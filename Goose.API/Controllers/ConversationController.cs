
using Goose.API.Services;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/issues/{issueId}/conversations")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IIssueConversationService _issueConversationService;

        public ConversationController(IIssueConversationService issueConversationService)
        {
            _issueConversationService = issueConversationService;
        }

        // GET: api/issues/{issueId}/conversations
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<IssueConversationDTO>>> Get([FromRoute] string issueId)
        {
            var issueConversations = await _issueConversationService.GetConversationsFromIssueAsync(issueId);
            return Ok(issueConversations);
        }

        // GET api/issues/{issueId}/conversations/{id}
        [HttpGet("{id}", Name = nameof(GetById))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IssueConversationDTO>> GetById([FromRoute] string issueId, string id)
        {
            var issueConversation = await _issueConversationService.GetConversationFromIssueAsync(issueId, id);
            return Ok(issueConversation);
        }

        // POST api/issues/{issueId}/conversations
        /// <summary>
        /// Use this Endpoint to write a Message in an Issue.
        /// Only Leader, Employee and Customer can write a Message.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromRoute] string issueId, [FromBody] IssueConversationDTO conversationItem)
        {
            if (conversationItem.Type != IssueConversation.MessageType)
            {
                throw new HttpStatusException(400, $"Invalid conversation type, only \"{IssueConversation.MessageType}\" allowed");
            }

            var newIssueConversation = await _issueConversationService.CreateNewIssueConversationAsync(issueId, conversationItem);
            return CreatedAtAction(nameof(GetById), new { issueId, id = newIssueConversation.Id }, newIssueConversation);
        }

        // PUT api/issues/{issueId}/conversations/{id}
        /// <summary>
        /// Use this Endpoint to update a Message in an Issue.
        /// Only Leader, Employee and Customer can update a Message.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromRoute] string issueId, [FromRoute] string id, [FromBody] IssueConversationDTO conversationItem)
        {
            if (conversationItem.Type != IssueConversation.MessageType)
            {
                throw new HttpStatusException(400, $"Invalid conversation type, only \"{IssueConversation.MessageType}\" allowed");
            }

            await _issueConversationService.CreateOrReplaceConversationItemAsync(issueId, id, conversationItem);
            return NoContent();
        }
    }
}
