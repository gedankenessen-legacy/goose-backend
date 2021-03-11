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
    [Route("api/[controller]")]
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
            ObjectId objectId = new ObjectId(id);
            var role = await _roleService.GetRoleAsync(objectId);
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRoleAsync([FromBody] Role role)
        {
            try
            {
                var newRole = await _roleService.CreateRoleAsync(role);
                return Ok(newRole);
            } 
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut]
        public async Task<ActionResult<Role>> UpdateRoleAsync([FromBody] Role role)
        {
            try
            {
                var roleToUpdate = await _roleService.UpdateRoleAsync(role);
                return Ok(roleToUpdate);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
