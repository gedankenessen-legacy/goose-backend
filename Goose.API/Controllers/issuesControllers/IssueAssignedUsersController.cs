using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
        public async Task<ActionResult<IList<UserDTO>>> GetAll([FromRoute] ObjectId issueId)
        {
            return Ok(await _issueService.GetAllOfIssueAsync(issueId));
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IList<UserDTO>>> Get([FromRoute] ObjectId issueId, [FromRoute] ObjectId userId)
        {
            var user = _issueService.GetAssignedUserOfIssueAsync(issueId, userId);
            if (user == null) return NotFound();
            return Ok(await user);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put([FromRoute] ObjectId issueId, [FromRoute] ObjectId id)
        {
            await _issueService.AssignUserAsync(issueId, id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete([FromRoute] ObjectId issueId, [FromRoute] ObjectId id)
        {
            await _issueService.UnassignUserAsync(issueId, id);
            return NoContent();
        }
    }
}