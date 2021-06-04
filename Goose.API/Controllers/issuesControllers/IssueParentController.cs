using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.issues;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("api/issues/{issueId}/parent")]
    [ApiController]
    public class IssueParentsController: Controller
    {
    private readonly IIssueParentService _issueParentService;

    public IssueParentsController(IIssueParentService issueParentService)
    {
        _issueParentService = issueParentService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<IssueDTO>>> Get([FromRoute] ObjectId issueId)
    {
        var issue = await _issueParentService.GetParent(issueId);
        if (issue == null) return Ok(new List<IssueDTO>());
        return Ok(issue);
    }

    [HttpPut("{parentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Put([FromRoute] ObjectId issueId, [FromRoute] ObjectId parentId)
    {
        await _issueParentService.SetParent(issueId, parentId);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete([FromRoute] ObjectId issueId)
    {
        await _issueParentService.RemoveParent(issueId);
        return NoContent();
    }
    }
}