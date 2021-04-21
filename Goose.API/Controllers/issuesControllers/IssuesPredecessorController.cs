using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs.Issues;
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IList<IssueDTO>> GetAll([FromRoute] ObjectId issueId)
        {
            return await _issueService.GetAll(issueId);
        }

        [HttpPut("{predecessorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SetPredecessor([FromRoute] ObjectId issueId, [FromRoute] ObjectId predecessorId)
        {
            await _issueService.SetPredecessor(issueId, predecessorId);
            return NoContent();
        }

        [HttpDelete("{predecessorId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RemovePredecessor([FromRoute] ObjectId issueId, [FromRoute] ObjectId predecessorId)
        {
            await _issueService.RemovePredecessor(issueId, predecessorId);
            return NoContent();
        }
    }
}