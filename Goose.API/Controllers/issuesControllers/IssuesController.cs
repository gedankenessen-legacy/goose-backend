using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
        public async Task<ActionResult<IList<IssueDTODetailed>>> GetAll([FromRoute] ObjectId projectId,
            [FromQuery] bool getAssignedUsers = false, [FromQuery] bool getConversations = false,
            [FromQuery] bool getTimeSheets = false, [FromQuery] bool getParent = false,
            [FromQuery] bool getPredecessors = false, [FromQuery] bool getSuccessors = false,
            [FromQuery] bool getAll = false)
        {
            var res = _issueDetailedService.GetAllOfProject(projectId, getAssignedUsers, getConversations,
                getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
            return Ok(await res);
        }

        //api/issues/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueDTODetailed>> Get([FromRoute] ObjectId projectId, [FromRoute] ObjectId id,
            [FromQuery] bool getAssignedUsers = false, [FromQuery] bool getConversations = false,
            [FromQuery] bool getTimeSheets = false, [FromQuery] bool getParent = false,
            [FromQuery] bool getPredecessors = false, [FromQuery] bool getSuccessors = false,
            [FromQuery] bool getAll = false)
        {
            var res = await _issueDetailedService.Get(projectId, id, getAssignedUsers,
                getConversations, getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
            return res == null ? NotFound() : Ok(res);
        }

        //api/issues
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromRoute] ObjectId projectId, [FromBody] IssueDTO dto)
        {
            if (dto.Project.Id != default && dto.Project.Id != projectId)
                throw new Exception("Project id must be the same in url and body or not defined in body");
            dto.Project.Id = projectId;

            var res = await _issueService.Create(dto);
            return CreatedAtAction(nameof(Get), new {projectId, id = res.Id}, res);
        }

        //api/issues/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromBody] IssueDTO issueRequest, [FromRoute] ObjectId id)
        {
            await _issueService.Update(issueRequest, id);
            return NoContent();
        }

        //api/issues/{id}  
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromRoute] ObjectId id)
        {
            if (await _issueService.Delete(id))
                return NoContent();
            return BadRequest();
        }
    }
}