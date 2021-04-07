using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Goose.API.Controllers.IssuesControllers
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetPredecessor([FromRoute] ObjectId projectId,
            [FromRoute] ObjectId issueId, [FromRoute] ObjectId predecessorId)
        {
            await _issueService.SetPredecessor(projectId, issueId,
                predecessorId);
            return NoContent();
        }

        [HttpDelete("{predecessorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemovePredecessor([FromRoute] ObjectId projectId,
            [FromRoute] ObjectId issueId, [FromRoute] ObjectId predecessorId)
        {
            await _issueService.RemovePredecessor(projectId, issueId,
                predecessorId);
            return NoContent();
        }
    }
}