using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Data;

namespace VolunteerComputing.TaskServer
{
    public static class PacketManager
    {
        public static void RemoveRange(this IList<Packet> packets, IEnumerable<Packet> toRemove)
        {
            foreach (var packet in toRemove)
            {
                packets.Remove(packet);
            }
        }

        public static Packet AggregatePackets(IList<Packet> packets)
        {
            var data = packets
                .Select(p => ShareAPI.GetTextFromShare(p.Data))
                .Aggregate((a, b) => $"{a},{b}");

            return new Packet
            {
                Type = packets.FirstOrDefault().Type,
                Data = $"[{data}]",
                Aggregated = true
            };
        }


        public static async Task<IEnumerable<PacketBundle>> CreateAndFindBundles(IEnumerable<DeviceData> devices, VolunteerComputingContext context)
        {
            var newPackets = context.Packets
                .Include(x => x.Type).ThenInclude(p => p.Project)
                .Where(x => x.BundleId == null && x.BundleResultId == null)
                .ToList();
            var tasks = context.ComputeTasks
                .Include(x => x.Project)
                .Include(x => x.PacketTypes).ThenInclude(x => x.PacketType)
                .ToList();

            var packetsFromFinishedBundles = await FindFinishedBundles(context);
            if (packetsFromFinishedBundles is null)
                return null;
            newPackets.AddRange(packetsFromFinishedBundles);

            var existingBundles = context.Bundles
                .Include(x => x.Packets)
                .Include(x => x.ComputeTask).ThenInclude(x => x.Project)
                .Include(x => x.BundleResults).ThenInclude(x => x.Packets).ThenInclude(x => x.DeviceWorkedOnIt)
                .Where(x => x.TimesSent < x.ComputeTask.Project.MinAgreeingClients)
                .ToList();
            foreach (var bundle in existingBundles.Where(b => b.TimesSent == 0))
            {
                var extendedPackets = bundle.Extend(newPackets);
                newPackets.RemoveRange(extendedPackets);
            }
            var newBundles = CreateBundles(newPackets, tasks, devices).ToList();
            context.Bundles.AddRange(newBundles);
            await context.SaveChangesAsync();

            return existingBundles.Concat(newBundles);
        }

        static async Task<IEnumerable<Packet>> FindFinishedBundles(VolunteerComputingContext context)
        {
            List<Packet> packets = new();
            var bundleReults = context.BundleResults
                .AsSplitQuery()
                .Include(x => x.Packets)
                .Include(x => x.Bundle)
                    .ThenInclude(x => x.ComputeTask).ThenInclude(x => x.Project)
                .Include(x => x.Bundle)
                    .ThenInclude(x => x.Packets)
                .AsEnumerable()
                .GroupBy(x => x.Bundle);
            foreach (var result in bundleReults)
            {
                using var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                try
                {
                    var bundle = result.Key;
                    var min = bundle?.ComputeTask.Project.MinAgreeingClients;
                    if (bundle.TimesSent != min || bundle.UntilCheck + min != result.Count())
                        continue;

                    var mostAgreedResult = result
                        .GroupBy(x => BitConverter.ToString(x.DataHash))
                        .OrderByDescending(x => x.Count())
                        .FirstOrDefault()
                        .ToList();
                    if (mostAgreedResult.Count < min)
                    {
                        Console.WriteLine($"Not enough volunteers agree on bundle {bundle.Id}, ({mostAgreedResult.Count}/{min}, results {result.Count()})");
                        bundle.UntilCheck++;
                        bundle.TimesSent--;
                        await context.SaveChangesAsync();
                        transaction.Commit();
                        continue;
                    }
                    var toKeep = mostAgreedResult.FirstOrDefault();
                    var toRemove = result
                        .Where(x => x.Id != toKeep.Id)
                        .SelectMany(x => x.Packets);

                    RemovePackets(context, toRemove);
                    foreach (var packet in toKeep.Packets)
                    {
                        packet.BundleId = null;
                        packet.Bundle = null;
                        packet.BundleResultId = null;
                        packet.BundleResult = null;
                        packet.DeviceWorkedOnIt = null;
                    }
                    context.BundleResults.RemoveRange(result);

                    RemovePackets(context, bundle.Packets);
                    context.Bundles.Remove(bundle);

                    packets.AddRange(toKeep.Packets);
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Rollbacking transaction due to: {ex.Message}");
                    transaction.Rollback();
                    return null;
                }
            }

            return packets;
        }

        static IEnumerable<PacketBundle> CreateBundles(List<Packet> packets, IEnumerable<ComputeTask> computeTasks, IEnumerable<DeviceData> devices)
        {
            var tempPackets = packets.ToList();
            foreach (var computeTask in computeTasks)
            {
                if (!devices.Any(d => d.CanCalculate(computeTask)))
                    break;

                while (true)
                {
                    var selectedPackets = new List<Packet>();
                    var inputPacketTypes = computeTask.PacketTypes
                        .Where(p => p.IsInput)
                        .OrderBy(p => p.Id)
                        .Select(p => p.PacketType)
                        .ToList();
                    var addedCount = 0;
                    foreach (var packetType in inputPacketTypes)
                    {
                        if (packetType.Aggregable)
                        {
                            var packetsToSelect = tempPackets.Where(t => t.Type == packetType).ToHashSet();
                            if (packetsToSelect.Count < 2)
                                break;
                            selectedPackets.AddRange(packetsToSelect);
                            tempPackets.RemoveAll(x => packetsToSelect.Contains(x));
                        }
                        else
                        {
                            var packetToSelect = tempPackets.FirstOrDefault(t => t.Type == packetType);
                            if (packetToSelect == null)
                                break;
                            selectedPackets.Add(packetToSelect);
                            tempPackets.Remove(packetToSelect);
                        }
                        addedCount++;
                    }
                    if (addedCount == inputPacketTypes.Count)
                        yield return new PacketBundle { ComputeTask = computeTask, Packets = selectedPackets, BundleResults = new List<BundleResult>() };
                    else
                        break;
                }
            }
        }

        static void RemovePackets(VolunteerComputingContext context, IEnumerable<Packet> packets, bool save = false)
        {
            foreach (var packet in packets)
            {
                ShareAPI.RemoveFromShare(packet.Data);
            }
            context.Packets.RemoveRange(packets);
            if (save)
                context.SaveChanges();
        }
    }
}
