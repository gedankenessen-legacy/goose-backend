using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IMessageService
    {
        public Task<IList<MessageDTO>> GetMessagesAsync();
        public Task<IList<MessageDTO>> GetMessagesFromUserAsync(ObjectId userId);
        public Task<MessageDTO> GetMessageDTOAsync(ObjectId messageId);
        public Task<MessageDTO> CreateMessageAsync(Message message);
        public Task<MessageDTO> UpdateMessageAsync(ObjectId messageId, MessageDTO message);
        public Task DeleteMessageAsync(ObjectId messageId);
        public Task DeleteAllUserMessage(ObjectId userId);
    }

    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<MessageDTO> CreateMessageAsync(Message message)
        {
            await _messageRepository.CreateAsync(message);
            return new MessageDTO(message);
        }

        public async Task DeleteMessageAsync(ObjectId messageId)
            => await _messageRepository.DeleteAsync(messageId);
        
        public async Task DeleteAllUserMessage(ObjectId userId)
            => await Task.WhenAll((await GetMessagesFromUserAsync(userId)).Select(message => _messageRepository.DeleteAsync(message.Id)));

        public async Task<MessageDTO> GetMessageDTOAsync(ObjectId messageId)
            => new MessageDTO(await GetMessageAsync(messageId));

        public async Task<IList<MessageDTO>> GetMessagesAsync()
            => (await _messageRepository.GetAsync()).Select(x => new MessageDTO(x)).ToList();

        public async Task<IList<MessageDTO>> GetMessagesFromUserAsync(ObjectId userId)
            => (await GetMessagesAsync()).Where(x => x.ReceiverUserId.Equals(userId)).ToList(); 

        public async Task<MessageDTO> UpdateMessageAsync(ObjectId messageId, MessageDTO message)
        {
            if (!messageId.Equals(message.Id))
                throw new HttpStatusException(400, "Die mitgebene ID stimmt nicht mit der message überein");

            var messageToUpdate = await GetMessageAsync(messageId);
            messageToUpdate.Consented = message.Consented;
            await _messageRepository.UpdateAsync(messageToUpdate);
            return await GetMessageDTOAsync(messageId);
        }

        private async Task<Message> GetMessageAsync(ObjectId messageId)
        {
            var message = (await _messageRepository.FilterByAsync(x => x.Id.Equals(messageId))).FirstOrDefault();

            if (message is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"There is no Message with the Id: {messageId}");

            return message;
        }
    }
}
