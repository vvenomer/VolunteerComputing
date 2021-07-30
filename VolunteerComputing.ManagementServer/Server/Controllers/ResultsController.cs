using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ByProject/{projectId}")]
        public ActionResult<List<Result>> GetResultsByProject(int projectId)
        {
            return _context.Result
                .Where(r => r.ProjectId == projectId)
                .ToList();
        }

        [HttpGet]
        public IActionResult GetResult([FromQuery] string fileId, [FromQuery] string projectName)
        {
            /*var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ResultsHelper.ReadResult(fileId, projectName))
            };
            var headers = result.Content.Headers;
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "result.json"
                };
            headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");*/
            return File(ResultsHelper.ReadResult(fileId, projectName), "application/octet-stream", "result.json");
        }
    }
}
