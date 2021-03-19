using Goose.API.Services;
using Goose.Data;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/project/{projectId}/users")]
    public class ProjectUserController : ControllerBase
    {
        private readonly IProjectUserService _projectUserService;

        public ProjectUserController(IProjectUserService projectUserService)
        {
            _projectUserService = projectUserService;
        }

        // PUT: api/project/{projectId}/user/{userId}
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProjectUser([FromBody] ProjectDTO projectDTO, string userId)
        {
            throw new NotImplementedException();
            //await _projectUserService.UpdateProjectUser(ObjectIdConverter.Validate(userId), projectDTO);
            //return NoContent();
        }

        // GET: api/project/{projectId}/user/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<ProjectDTO>>> GetProjectUsers([FromRoute] string projectId)
        {
            var projectIter = await _projectUserService.GetProjectUsers(ObjectIdConverter.Validate(projectId));
            return Ok(projectIter);
        }

        // GET: api/project/{projectId}/user/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDTO>> GetProjectUser([FromRoute] string projectId, string userId)
        {
            var projectUser = await _projectUserService.GetProjectUser(
                ObjectIdConverter.Validate(projectId),
                ObjectIdConverter.Validate(userId));

            if (projectUser == null)
            {
                return Ok(projectUser);
            }
            else
            {
                return NoContent();
            }
        }
    }
}
