using Goose.API.Services;
using Goose.API.Utils;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<MessageDTO>>> GetMessagesAsync()
        {
            var messageList = await _messageService.GetMessagesAsync();
            return Ok(messageList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IList<MessageDTO>>> GetUserMessagesAsync(string id)
        {
            var messageList = await _messageService.GetMessagesFromUserAsync(id.ToObjectId());
            return Ok(messageList);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessageAsync([FromBody] Message message)
        {
            var newMessage = await _messageService.CreateMessageAsync(message);
            return Ok(newMessage);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageDTO>> UpdateMessageAsync(string id, [FromBody] MessageDTO message)
        {
            var messageToUpdate = await _messageService.UpdateMessageAsync(id.ToObjectId(), message);
            return Ok(messageToUpdate);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessageAsync(string id)
        {
            await _messageService.DeleteMessageAsync(id.ToObjectId());
            return Ok();
        }
    }
}
