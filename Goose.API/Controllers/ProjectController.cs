using Goose.API.Services;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // POST: api/project
        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> CreateProject([FromBody] ProjectDTO projectDTO)
        {
            var newCompany = await _projectService.CreateNewProjectAsync(projectDTO);
            return Ok(newCompany); 
        }

        // PUT: api/project/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProject(string id, [FromBody] ProjectDTO projectDTO)
        {
            await _projectService.UpdateProject(id, projectDTO);
            return Ok();
        }

        // GET: api/project
        [HttpGet]
        public async Task<ActionResult<IList<ProjectDTO>>> GetProjects()
        {
            var projectIter = await _projectService.GetProjects();
            return Ok(projectIter);
        }

        // GET: api/project/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(string id)
        {
            var projects = await _projectService.GetProject(id);
            return Ok(projects);
        }
    }
}
