using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public DevicesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IList<DeviceData>> GetDevices()
        {
            return dbContext.Devices.Include(d => d.DeviceStats).ToList();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDevices()
        {
            var packets = dbContext.Packets.Include(p => p.DeviceWorkedOnIt).Where(p => p.DeviceWorkedOnIt != null);
            foreach (var packet in packets)
            {
                packet.DeviceWorkedOnIt = null;
            }
            dbContext.DeviceStats.RemoveRange(dbContext.DeviceStats);
            dbContext.Devices.RemoveRange(dbContext.Devices);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("DeleteStats")]
        public async Task<IActionResult> DeleteStats()
        {
            dbContext.DeviceStats.RemoveRange(dbContext.DeviceStats);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
