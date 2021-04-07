using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers.IssuesControllers
{
    [Route("api/issues/{issueId}/users/")]
    [ApiController]
    public class IssueAssignedUsersController : Controller
    {
        private readonly IIssueAssignedUserService _issueService;

        public IssueAssignedUsersController(IIssueAssignedUserService issueService)
        {
            _issueService = issueService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<UserDTO>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IList<UserDTO>>> Get([FromRoute] string issueId, [FromRoute] string userId)
        {
            var user = _issueService.GetAssignedUserOfIssueAsync(issueId.ToObjectId(), userId.ToObjectId());
            if (user == null) return NotFound();
            return Ok(await user);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.AssignUserAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.UnassignUserAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}