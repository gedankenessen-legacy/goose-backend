using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/issues/{issueId}/timesheets/")]
    [ApiController]
    public class IssueTimeSheetsController : Controller
    {
        private readonly IIssueTimeSheetService _issueService;

        public IssueTimeSheetsController(IIssueTimeSheetService issueRepo)
        {
            _issueService = issueRepo;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}")]
        public async Task<ActionResult<IssueTimeSheetDTO>> Get([FromRoute] string issueId, [FromRoute] string id)
        {
            return Ok(await _issueService.GetAsync(issueId.ToObjectId(), id.ToObjectId()));
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Post([FromRoute] string issueId,
            [FromBody] IssueTimeSheetDTO timeSheetDto)
        {
            var res = await _issueService.CreateAsync(issueId.ToObjectId(), timeSheetDto);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Put([FromRoute] string id,
            [FromBody] IssueTimeSheetDTO timeSheetDto)
        {
            await _issueService.UpdateAsync(id.ToObjectId(), timeSheetDto);
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Delete([FromRoute] string issueId,
            [FromRoute] string id)
        {
            await _issueService.DeleteAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}