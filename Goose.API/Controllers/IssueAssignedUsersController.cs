using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services;
using Goose.Domain.Models.identity;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers
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
        public async Task<ActionResult<IList<User>>> GetAll([FromRoute] string issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId.ToObjectId()));
        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<IList<User>>> GetAll([FromRoute] string issueId, [FromRoute] string userId)
        {
            var user = _issueService.GetAssignedUserOfIssueAsync(issueId.ToObjectId(), userId: userId.ToObjectId());
            if (user == null) return NotFound();
            return Ok(await user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.AssignUserAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.UnassignUserAsync(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}