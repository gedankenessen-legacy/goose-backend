using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs.issues;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/issues/")]
    [ApiController]
    public class IssueTimeSheetsController : Controller
    {
        private readonly IIssueTimeSheetService _issueService;

        public IssueTimeSheetsController(IIssueTimeSheetService issueRepo)
        {
            _issueService = issueRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IssueTimeSheetDTO>> Get([FromRoute] string issueId, [FromRoute] string id)
        {
            return Ok(await _issueService.GetAsync(issueId.ToObjectId(), id.ToObjectId()));
        }

        [HttpPost]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Post([FromRoute] string issueId,
            [FromBody] IssueTimeSheetDTO requirement)
        {
            var res = await _issueService.CreateAsync(issueId.ToObjectId(), requirement);
            return CreatedAtAction(nameof(Get), new {id = res.Id}, res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Put([FromRoute] string id,
            [FromBody] IssueTimeSheetDTO requirement)
        {
            await _issueService.UpdateAsync(id.ToObjectId(), requirement);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<IList<IssueTimeSheetDTO>>> Delete([FromRoute] string issueId,
            [FromRoute] string id)
        {
            await _issueService.DeleteAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}