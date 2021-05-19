using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacketTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PacketTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PacketTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacketType>>> GetPacketTypes()
        {
            return await _context.PacketTypes.ToListAsync();
        }

        // GET: api/PacketTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PacketType>> GetPacketType(int id)
        {
            var packetType = await _context.PacketTypes.FindAsync(id);

            if (packetType == null)
            {
                return NotFound();
            }

            return packetType;
        }

        // PUT: api/PacketTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPacketType(int id, PacketType packetType)
        {
            if (id != packetType.Id)
            {
                return BadRequest();
            }

            _context.Entry(packetType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PacketTypeExists(id))
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

        // POST: api/PacketTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<List<PacketType>> PostPacketTypes(List<PacketType> packetTypes)
        {
            foreach (var packetType in packetTypes)
            {
                var project = _context.Projects.FirstOrDefault(p => p.Name == packetType.Project.Name);
                if(project is not null)
                    packetType.Project = project;

                _context.PacketTypes.Add(packetType);
                await _context.SaveChangesAsync();
            }

            return packetTypes;
        }

        // DELETE: api/PacketTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePacketType(int id)
        {
            var packetType = await _context.PacketTypes.FindAsync(id);
            if (packetType == null)
            {
                return NotFound();
            }

            _context.PacketTypes.Remove(packetType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PacketTypeExists(int id)
        {
            return _context.PacketTypes.Any(e => e.Id == id);
        }
    }
}
