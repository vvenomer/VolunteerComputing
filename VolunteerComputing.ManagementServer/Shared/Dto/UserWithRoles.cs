using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Dto
{
    public class UserWithRoles
    {
        public UserWithRoles()
        {
        }
        public UserWithRoles(string name, IEnumerable<string> roles)
        {
            Name = name;
            Roles = roles.ToHashSet();
        }

        public string Name { get; set; }
        public HashSet<string> Roles { get; set; }
    }
}
