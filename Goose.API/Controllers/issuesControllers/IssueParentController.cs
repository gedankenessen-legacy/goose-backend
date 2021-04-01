using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.issuesControllers
{
    [Route("api/issues/{issueId}/parent")]
    [ApiController]
    public class IssueParentsController: Controller
    {
    private readonly IIssueService _issueService;

    public IssueParentsController(IIssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<UserDTO>>> Get([FromRoute] string issueId)
    {
        var issue = await _issueService.GetParent(issueId.ToObjectId());
        if (issue == null) return NotFound();
        return Ok(issue);
    }

    [HttpPut("{parentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Put([FromRoute] string issueId, [FromRoute] string parentId)
    {
        await _issueService.SetParent(issueId.ToObjectId(), parentId.ToObjectId());
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete([FromRoute] string issueId)
    {
        await _issueService.RemoveParent(issueId.ToObjectId());
        return NoContent();
    }
    }
}