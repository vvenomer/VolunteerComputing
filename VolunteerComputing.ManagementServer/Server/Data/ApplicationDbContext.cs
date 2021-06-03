using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VolunteerComputing.ManagementServer.Server.Models;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public virtual DbSet<PacketType> PacketTypes { get; set; }
        public virtual DbSet<ComputeTask> ComputeTask { get; set; }
        public virtual DbSet<Packet> Packets { get; set; }
        public virtual DbSet<PacketTypeToComputeTask> PacketTypeToComputeTasks { get; set; }
        public virtual DbSet<DeviceData> Devices { get; set; }
        public virtual DbSet<DeviceStat> DeviceStats { get; set; }
        public virtual DbSet<Project> Projects { get; set; }

        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
    }
}
