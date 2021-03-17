using Goose.API.Services;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/company/{companyId}/project")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // POST: api/company/{companyId}/project
        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> CreateProject([FromBody] ProjectDTO projectDTO, [FromRoute] ObjectId companyId)
        {
            var newCompany = await _projectService.CreateNewProjectAsync(companyId, projectDTO);
            return Ok(newCompany); 
        }

        // PUT: api/company/{companyId}/project/{projectId}
        [HttpPut("{projectId}")]
        public async Task<ActionResult> UpdateProject(ObjectId projectId, [FromBody] ProjectDTO projectDTO)
        {
            await _projectService.UpdateProject(projectId, projectDTO);
            return NoContent();
        }

        // GET: api/company/{companyId}/project
        [HttpGet]
        public async Task<ActionResult<IList<ProjectDTO>>> GetProjects([FromRoute] ObjectId companyId)
        {
            var projectIter = await _projectService.GetProjects();
            return Ok(projectIter);
        }

        // GET: api/company/{companyId}/project/{projectId}
        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(ObjectId projectId)
        {
            var projects = await _projectService.GetProject(projectId);
            return Ok(projects);
        }
    }
}
