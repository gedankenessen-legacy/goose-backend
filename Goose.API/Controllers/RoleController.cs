using Goose.API.Services;
using Goose.Domain.Models.identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/role")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<Role>>> GetRolesAsync()
        {
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRolesAsync(string id)
        {
            var role = await _roleService.GetRoleAsync(new ObjectId(id));
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRoleAsync([FromBody] Role role)
        {
            var newRole = await _roleService.CreateRoleAsync(role);
            return Ok(newRole);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Role>> UpdateRoleAsync(string id, [FromBody] Role role)
        {
            var roleToUpdate = await _roleService.UpdateRoleAsync(new ObjectId(id), role);
            return Ok(roleToUpdate);
        }
    }
}
