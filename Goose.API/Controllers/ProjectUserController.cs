using Goose.API.Services;
using Goose.API.Utils.Validators;
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
    [Route("api/projects/{projectId}/users")]
    [ApiController]
    public class ProjectUserController : ControllerBase
    {
        private readonly IProjectUserService _projectUserService;

        public ProjectUserController(IProjectUserService projectUserService)
        {
            _projectUserService = projectUserService;
        }

        // PUT: api/projects/{projectId}/users/{userId}
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IList<PropertyUserDTO>>> UpdateProjectUser([FromBody] PropertyUserDTO projectUserDTO, [FromRoute] ObjectId projectId, ObjectId userId)
        {
           var users =  await _projectUserService.UpdateProjectUser(projectId, userId, projectUserDTO);

            return Ok(users);
        }

        // GET: api/projects/{projectId}/users/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<PropertyUserDTO>>> GetProjectUsers([FromRoute] ObjectId projectId)
        {
            var projectIter = await _projectUserService.GetProjectUsers(projectId);
            return Ok(projectIter);
        }

        // GET: api/projects/{projectId}/users/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropertyUserDTO>> GetProjectUser([FromRoute] ObjectId projectId, ObjectId userId)
        {
            var projectUser = await _projectUserService.GetProjectUser(projectId, userId);

            if (projectUser != null)
            {
                return Ok(projectUser);
            }
            else
            {
                return NotFound();
            }
        }

        // DELETE: api/projects/{projectId}/users/{userId}
        // Dies entfernt nur den User vom Projekt, der eigentliche User bleibt bestehen.
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUserFromProject([FromRoute] ObjectId projectId, ObjectId userId)
        {
            await _projectUserService.RemoveUserFromProject(projectId, userId);

            return Ok();
        }

    }
}
