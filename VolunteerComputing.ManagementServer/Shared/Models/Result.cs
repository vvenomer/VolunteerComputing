using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class Result
    {
        public int Id { get; set; }
        public string FileId { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
