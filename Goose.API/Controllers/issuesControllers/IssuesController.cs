using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs.Issues;
using Goose.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("api/projects/{projectId}/issues/")]
    [ApiController]
    public class IssuesController : Controller
    {
        private readonly IIssueService _issueService;
        private readonly IIssueDetailedService _issueDetailedService;

        public IssuesController(IIssueService issueService, IIssueDetailedService issueDetailedService)
        {
            _issueService = issueService;
            _issueDetailedService = issueDetailedService;
        }

        //api/issues/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<IssueDTODetailed>>> GetAll([FromRoute] string projectId,
            [FromQuery] bool getAssignedUsers = false, [FromQuery] bool getConversations = false,
            [FromQuery] bool getTimeSheets = false, [FromQuery] bool getParent = false,
            [FromQuery] bool getPredecessors = false, [FromQuery] bool getSuccessors = false,
            [FromQuery] bool getAll = false)
        {
            var res = _issueDetailedService.GetAllOfProject(projectId.ToObjectId(), getAssignedUsers, getConversations,
                getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
            return Ok(await res);
        }

        //api/issues/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueDTODetailed>> Get([FromRoute] string projectId, [FromRoute] string id,
            [FromQuery] bool getAssignedUsers = false, [FromQuery] bool getConversations = false,
            [FromQuery] bool getTimeSheets = false, [FromQuery] bool getParent = false,
            [FromQuery] bool getPredecessors = false, [FromQuery] bool getSuccessors = false,
            [FromQuery] bool getAll = false)
        {
            var res = await _issueDetailedService.Get(projectId.ToObjectId(), id.ToObjectId(), getAssignedUsers,
                getConversations, getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
            return res == null ? NotFound() : Ok(res);
        }

        //api/issues
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromRoute] string projectId, [FromBody] IssueDTO dto)
        {
            if (dto.Project.Id != default && dto.Project.Id != projectId.ToObjectId())
                throw new Exception($"Project id must be the same in url ({projectId}) and body ({dto.Project.Id}) or not defined in body");
            dto.Project.Id = projectId.ToObjectId();

            var res = await _issueService.Create(dto);
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