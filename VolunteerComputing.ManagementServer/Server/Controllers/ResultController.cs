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
    public class ResultController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ResultController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<string> GetResultsForProject(Project project)
        {
            return _context.Result
                .Where(r => r.Project == project)
                .Select(r => r.FileId);
        }

        public HttpResponseMessage GetResult(string fileId, string projectName)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(ResultsHelper.ReadResult(fileId, projectName))
            };
            var headers = result.Content.Headers;
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "result.json"
                };
            headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }
    }
}
