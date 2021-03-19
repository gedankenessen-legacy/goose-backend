﻿using Goose.API.Services;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDTO>> CreateProject([FromBody] ProjectDTO projectDTO, [FromRoute] string companyId)
        {

            var newCompany = await _projectService.CreateProjectAsync(ObjectIdConverter.Validate(companyId), projectDTO);
            return Ok(newCompany); 
        }

        // PUT: api/company/{companyId}/project/{projectId}
        [HttpPut("{projectId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProject([FromBody] ProjectDTO projectDTO, string projectId)
        {
            await _projectService.UpdateProject(ObjectIdConverter.Validate(projectId), projectDTO);
            return NoContent();
        }

        // GET: api/company/{companyId}/project
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<ProjectDTO>>> GetProjects()
        {
            var projectIter = await _projectService.GetProjects();
            return Ok(projectIter);
        }

        // GET: api/company/{companyId}/project/{projectId}
        [HttpGet("{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDTO>> GetProject(string projectId)
        {
            var projects = await _projectService.GetProject(ObjectIdConverter.Validate(projectId));
            return Ok(projects);
        }
    }
}
