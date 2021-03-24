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
    [Route("api/companies/{companyId}/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // POST: api/companies/{companyId}/projects
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDTO>> CreateProject([FromBody] ProjectDTO projectDTO, [FromRoute] ObjectId companyId)
        {
            var newCompany = await _projectService.CreateProjectAsync(companyId, projectDTO);
            return Ok(newCompany); 
        }

        // PUT: api/companies/{companyId}/projects/{projectId}
        [HttpPut("{projectId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProject([FromBody] ProjectDTO projectDTO, ObjectId projectId)
        {
            await _projectService.UpdateProject(projectId, projectDTO);
            return NoContent();
        }

        // GET: api/companies/{companyId}/projects
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<ProjectDTO>>> GetProjects()
        {
            var projectIter = await _projectService.GetProjects();
            return Ok(projectIter);
        }

        // GET: api/companies/{companyId}/projects/{projectId}
        [HttpGet("{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDTO>> GetProject(ObjectId projectId)
        {
            var projects = await _projectService.GetProject(projectId);
            return Ok(projects);
        }
    }
}
