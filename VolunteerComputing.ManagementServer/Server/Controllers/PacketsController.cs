using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.ManagementServer.Server.Hubs;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacketsController : ControllerBase
    {
        readonly ApplicationDbContext _context;
        readonly IHubContext<TaskManagementHub, ITaskManagementHubMessages> hubContext;

        public PacketsController(ApplicationDbContext context, IHubContext<TaskManagementHub, ITaskManagementHubMessages> hubContext)
        {
            _context = context;
            this.hubContext = hubContext;
        }

        // GET: api/Packets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Packet>>> GetPackets()
        {
            return await _context.Packets.ToListAsync();
        }

        // GET: api/Packets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Packet>> GetPacket(long id)
        {
            var packet = await _context.Packets.FindAsync(id);

            if (packet == null)
            {
                return NotFound();
            }

            return packet;
        }

        // PUT: api/Packets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPacket(long id, Packet packet)
        {
            if (id != packet.Id)
            {
                return BadRequest();
            }

            _context.Entry(packet).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PacketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Packets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Packet>> PostPacket(Packet packet)
        {
            var type = packet.Type;
            var projectName = type.Project.Name;
            packet.Type = _context.PacketTypes.FirstOrDefault(t => t.Type == type.Type && t.Project.Name == projectName);
            packet.Data = ShareAPI.SaveTextToShare(packet.Data);
            _context.Packets.Add(packet);
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            project.StartTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            await hubContext.Clients.Group(TaskManagementHub.taskServerId).PacketAdded();

            return CreatedAtAction("GetPacket", new { id = packet.Id }, packet);
        }

        // DELETE: api/Packets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePacket(long id)
        {
            var packet = await _context.Packets.FindAsync(id);
            if (packet == null)
            {
                return NotFound();
            }

            _context.Packets.Remove(packet); //todo: delete from share
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("DeleteByProject/{projectId}")]
        public async Task<IActionResult> DeleteByProject(long projectId)
        {
            var project = await _context.Projects
                .AsSplitQuery()
                .Include(p => p.PacketTypes)
                    .ThenInclude(t => t.Packets)
                        .ThenInclude(p => p.Bundle)
                        .ThenInclude(p => p.BundleResults)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }
            var packets = project.PacketTypes
                .SelectMany(p => p.Packets)
                .ToList();
            
            foreach (var packet in packets)
            {
                ShareAPI.RemoveFromShare(packet.Data);
            }
            _context.Packets.RemoveRange(packets);
            _context.BundleResults.RemoveRange(packets.Where(p => p.BundleResult != null).Select(p => p.BundleResult).Distinct());
            _context.Bundles.RemoveRange(packets.Where(p => p.Bundle != null).Select(p => p.Bundle).Distinct());
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PacketExists(long id)
        {
            return _context.Packets.Any(e => e.Id == id);
        }
    }
}
