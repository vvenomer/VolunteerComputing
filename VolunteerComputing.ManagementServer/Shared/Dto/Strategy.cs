using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.Shared.Dto
{
    public class Strategy
    {
        public ChoosingStrategy ChoosingStrategy { get; set; }
        public double ChanceToUseNewDevice { get; set; }
    }
}
