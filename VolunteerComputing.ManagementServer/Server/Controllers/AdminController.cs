using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.ManagementServer.Server.Models;
using VolunteerComputing.Shared.Dto;

namespace VolunteerComputing.ManagementServer.Server.Controllers
{
    [Route("api/Admins")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet("GetRoles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            return await roleManager.Roles.Select(x => x.Name).ToListAsync();
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<UserWithRoles>>> GetUsers()
        {
            return await Task.WhenAll(
                userManager.Users.ToList()
                    .Select(async x => new UserWithRoles(x.UserName, await userManager.GetRolesAsync(x))));
        }

        [HttpPost("UpdateUserRoles")]
        public async Task<ActionResult> UpdateUserRoles([FromBody] UserWithRoles userWithRoles)
        {
            var user = await userManager.FindByNameAsync(userWithRoles.Name);
            foreach (var role in roleManager.Roles.Select(x => x.Name).ToList())
            {
                if (await userManager.IsInRoleAsync(user, role))
                {
                    if (!userWithRoles.Roles.Contains(role))
                    {
                        await userManager.RemoveFromRoleAsync(user, role);
                    }
                }
                else if (userWithRoles.Roles.Contains(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

            }
            return Ok();
        }
    }
}
