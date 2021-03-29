using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/projects/{projectId}/issues/")]
    [ApiController]
    public class IssuesController : Controller
    {
        private readonly IIssueService _issueService;

        public IssuesController(IIssueService issueService)
        {
            _issueService = issueService;
        }

        //api/issues/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<IssueDTO>>> GetAll([FromRoute] string projectId)
        {
            var res = await _issueService.GetAllOfProject(new ObjectId(projectId));
            return Ok(res);
        }

        //api/issues/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IssueDTO>> Get([FromRoute] string projectId, [FromRoute] string id)
        {
            var res = await _issueService.GetOfProject(projectId.ToObjectId(), id.ToObjectId());
            return res == null ? NotFound() : Ok(res);
        }

        //api/issues
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromRoute] string projectId, [FromBody] IssueCreateDTO issueRequest)
        {
            if (issueRequest.ProjectId != default && issueRequest.ProjectId != projectId.ToObjectId())
                throw new Exception("Project id must be the same in url and body or not defined in body");
            issueRequest.ProjectId = projectId.ToObjectId();

            var res = await _issueService.Create(issueRequest);
            return CreatedAtAction(nameof(Get), new {projectId, id = res.Id}, res);
        }

        //api/issues/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] IssueDTO issueRequest, [FromRoute] string id)
        {
            await _issueService.Update(issueRequest, id.ToObjectId());
            return NoContent();
        }

        //api/issues/{id}  
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromRoute] string id)
        {
            if (await _issueService.Delete(id.ToObjectId()))
                return NoContent();
            return BadRequest();
        }
    }
}