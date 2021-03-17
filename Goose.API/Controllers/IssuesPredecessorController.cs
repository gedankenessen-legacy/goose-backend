using System;
using System.Threading.Tasks;
using Goose.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Goose.API.Controllers
{
    [Route("api/issues/{issueId}/predecessors/")]
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
            [FromRoute] string issueId, [FromRoute] string predecessorId)
        {
            await _issueService.SetPredecessor(projectId.ToObjectId(), issueId.ToObjectId(),
                predecessorId.ToObjectId());
            return NoContent();
        }

        [HttpDelete("{predecessorId}")]
        public async Task<ActionResult> RemovePredecessor([FromRoute] string projectId,
            [FromRoute] string issueId, [FromRoute] string predecessorId)
        {
            await _issueService.RemovePredecessor(projectId.ToObjectId(), issueId.ToObjectId(),
                predecessorId.ToObjectId());
            return NoContent();
        }
    }
}