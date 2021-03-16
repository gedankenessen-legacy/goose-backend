using System;
using System.Threading.Tasks;
using Goose.API.Services;
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

        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.AssignUser(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] string issueId, [FromRoute] string id)
        {
            await _issueService.UnassignUser(issueId.ToObjectId(), id.ToObjectId());
            return NoContent();
        }
    }
}