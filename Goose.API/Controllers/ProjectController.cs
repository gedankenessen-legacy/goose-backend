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
        public async Task<ActionResult<ProjectDTO>> UpdateProject(string id, [FromBody] ProjectDTO projectDTO)
        {
            var updatedProject = await _projectService.UpdateProject(id, projectDTO);
            return Ok(updatedProject);
        }

        // GET: api/project
        [HttpGet]
        public async Task<ActionResult<ProjectDTO[]>> GetProjects()
        {
            var projectIter = _projectService.GetProjects();
            var projects = await projectIter.ToArrayAsync();
            return Ok(projects);
        }

        // GET: api/project/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetProjects(string id)
        {
            var projects = await _projectService.GetProject(id);
            return Ok(projects);
        }
    }
}
