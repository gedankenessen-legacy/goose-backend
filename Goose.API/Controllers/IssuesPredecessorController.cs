using System;
using System.Threading.Tasks;
using Goose.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers
{
    [Route("api/projects/{projectId}/issues/{issueId}/predecessors/")]
    [ApiController]
    public class IssuesPredecessorController : Controller
    {
        private readonly IIssuePredecessorService _issueService;

        public IssuesPredecessorController(IIssuePredecessorService issueService)
        {
            _issueService = issueService;
        }

        [HttpPut("{predecessorId}")]
        public async Task<ActionResult> SetPredecessor([FromRoute] string projectId,
            [FromRoute] string issueId, [FromBody] string predecessorId)
        {
            await _issueService.SetPredecessor(projectId.ToObjectId(), issueId.ToObjectId(),
                predecessorId.ToObjectId());
            return NoContent();
        }

        [HttpDelete("{predecessorId}")]
        public async Task<ActionResult> RemovePredecessor([FromRoute] string projectId,
            [FromRoute] string issueId, [FromBody] string predecessorId)
        {
            await _issueService.RemovePredecessor(projectId.ToObjectId(), issueId.ToObjectId(),
                predecessorId.ToObjectId());
            return NoContent();
        }
    }
}