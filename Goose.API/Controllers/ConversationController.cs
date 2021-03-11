
using Goose.API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/company/{companyId}/project/{projectId}/issue/{issueId}/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IIssueConversationService _issueConversationService;

        public ConversationController(IIssueConversationService issueConversationService)
        {
            _issueConversationService = issueConversationService;
        }

        // GET: api/company/{companyId}/project/{projectId}/issue/{issueId}/<ConversationController>/
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string issueId)
        {
            var issueConversations = await _issueConversationService.GetConversationsFromIssueAsync(issueId);
            return Ok(issueConversations);
        }

        // GET api/company/{companyId}/project/{projectId}/issue/{issueId}/<ConversationController>/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] string issueId, string id)
        {
            var issueConversation = await _issueConversationService.GetConversationFromIssueAsync(issueId, id);

            return Ok(issueConversation);
        }

        // POST api/company/{companyId}/project/{projectId}/issue/{issueId}/<ConversationController>/
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/company/{companyId}/project/{projectId}/issue/{issueId}/<ConversationController>/{id}
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // NOT IN SPECIFICATIONS
        // DELETE api/company/{companyId}/project/{projectId}/issue/{issueId}/<ConversationController>/{id}
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
