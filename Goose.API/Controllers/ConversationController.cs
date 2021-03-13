
using Goose.API.Services;
using Goose.Domain.DTOs.tickets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/issue/{issueId}/conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IIssueConversationService _issueConversationService;

        public ConversationController(IIssueConversationService issueConversationService)
        {
            _issueConversationService = issueConversationService;
        }

        // GET: api/issue/{issueId}/conversation
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromRoute] string issueId)
        {
            var issueConversations = await _issueConversationService.GetConversationsFromIssueAsync(issueId);
            return Ok(issueConversations);
        }

        // GET api/issue/{issueId}/conversation/{id}
        [HttpGet("{id}", Name = nameof(GetById))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById([FromRoute] string issueId, string id)
        {
            var issueConversation = await _issueConversationService.GetConversationFromIssueAsync(issueId, id);
            return Ok(issueConversation);
        }

        // POST api/issue/{issueId}/conversation
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromRoute] string issueId, [FromBody] IssueConversationDTO conversationItem)
        {
            var newIssueConversation = await _issueConversationService.CreateNewIssueConversationAsync(issueId, conversationItem);
            return CreatedAtAction(nameof(GetById), new { issueId, id = newIssueConversation.Id }, newIssueConversation);
        }

        // PUT api/issue/{issueId}/conversation/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromRoute] string issueId, [FromRoute] string id, [FromBody] IssueConversationDTO conversationItem)
        {
            await _issueConversationService.CreateOrReplaceConversationItemAsync(issueId, id, conversationItem);
            return NoContent();
        }
    }
}
