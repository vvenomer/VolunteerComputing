using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputeTasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ComputeTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ComputeTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComputeTask>>> GetComputeTask()
        {
            return await _context.ComputeTask.Include(p => p.PacketTypes).ToListAsync();
        }

        // GET: api/ComputeTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ComputeTask>> GetComputeTask(int id)
        {
            var computeTask = await _context.ComputeTask.Include(p => p.PacketTypes).FirstAsync(p => p.Id == id);

            if (computeTask == null)
            {
                return NotFound();
            }

            return computeTask;
        }

        [HttpGet("ByProject/{projectId}")]
        public ActionResult<IEnumerable<ComputeTask>> GetComputeTasksByProject(int projectId)
        {
            var project =  _context.Projects
                .AsSplitQuery()
                .Include(p => p.ComputeTasks)
                .ThenInclude(t => t.PacketTypes)
                .ThenInclude(x => x.PacketType)
                .Where(p => p.Id == projectId)
                .AsEnumerable()
                .FirstOrDefault();
            if (project is null)
                return NotFound();
            return project.ComputeTasks.ToList();
        }

        // PUT: api/ComputeTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComputeTask(int id, ComputeTask computeTask)
        {
            if (id != computeTask.Id)
            {
                return BadRequest();
            }

            UpdatePacketTypes(computeTask.PacketTypes, computeTask);
            _context.Entry(computeTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputeTaskExists(id))
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

        // POST: api/ComputeTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<List<ComputeTask>> PostComputeTasks(List<ComputeTask> computeTasks, [FromQuery] int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            foreach (var computeTask in computeTasks)
            {
                string path;
                if (computeTask.WindowsCpuProgram is not null)
                {
                    path = ShareAPI.SaveToShare(computeTask.WindowsCpuProgram);
                    computeTask.WindowsCpuProgram = path;
                }
                if (computeTask.WindowsGpuProgram is not null)
                {
                    path = ShareAPI.SaveToShare(computeTask.WindowsGpuProgram);
                    computeTask.WindowsGpuProgram = path;
                }
                if (computeTask.LinuxCpuProgram is not null)
                {
                    path = ShareAPI.SaveToShare(computeTask.LinuxCpuProgram);
                    computeTask.LinuxCpuProgram = path;
                }
                if (computeTask.LinuxGpuProgram is not null)
                {
                    path = ShareAPI.SaveToShare(computeTask.LinuxGpuProgram);
                    computeTask.LinuxGpuProgram = path;
                }

                var packetTypes = computeTask.PacketTypes;
                computeTask.PacketTypes = null;
                computeTask.Project = project;
                _context.ComputeTask.Add(computeTask);
                UpdatePacketTypes(packetTypes, computeTask);
            }
            await _context.SaveChangesAsync();

            return computeTasks;
        }

        // DELETE: api/ComputeTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComputeTask(int id)
        {
            var computeTask = await _context.ComputeTask.FindAsync(id);
            if (computeTask == null)
            {
                return NotFound();
            }
            foreach (var packetType in computeTask.PacketTypes)
            {
                _context.PacketTypeToComputeTasks.Remove(packetType);
            }
            _context.ComputeTask.Remove(computeTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComputeTaskExists(int id)
        {
            return _context.ComputeTask.Any(e => e.Id == id);
        }

        void UpdatePacketTypes(ICollection<PacketTypeToComputeTask> packetTypes, ComputeTask computeTask)
        {
            foreach (var packetType in packetTypes)
            {
                var type = packetType.PacketType.Type;
                var projectName = packetType.PacketType.Project.Name;
                packetType.PacketType = _context.PacketTypes.FirstOrDefault(t => t.Type == type && t.Project.Name == projectName);

                packetType.ComputeTask = computeTask;

                if (packetType.Id != 0)
                    _context.Entry(packetType).State = EntityState.Modified;
                else
                    _context.PacketTypeToComputeTasks.Add(packetType);
            }
        }
    }
}
