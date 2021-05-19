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
        public DbSet<PacketType> PacketTypes { get; set; }
        public DbSet<ComputeTask> ComputeTask { get; set; }
        public DbSet<Packet> Packets { get; set; }
        public DbSet<PacketTypeToComputeTask> PacketTypeToComputeTasks { get; set; }
        public DbSet<DeviceData> Devices { get; set; }
        public DbSet<Project> Projects { get; set; }

        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }
    }
}
