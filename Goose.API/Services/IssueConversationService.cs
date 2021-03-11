using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.tickets;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IIssueConversationService
    {
        public Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId);
        public Task<IssueConversationDTO> GetConversationFromIssueAsync(string issueId, string conversationId);
    }

    public class IssueConversationService : IIssueConversationService
    {
        private readonly IIssueRepository _issueRepository;

        public IssueConversationService(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<IssueConversationDTO> GetConversationFromIssueAsync(string issueId, string conversationId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(issueId, out ObjectId issueOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");

            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(conversationId, out ObjectId conversationOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse conversation string id to a valid object id.");

            // fetch the issue that contains the conversation.
            Issue issue = await _issueRepository.GetAsync(issueOid);

            // show error if issue is not found.
            if (issue is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Issue found with Id={ issueId }");

            if (issue.ConversationItems is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "No conversation items found.");

            var conversationItem = issue.ConversationItems.Single(ci => ci.Id.Equals(conversationOid));

            if (conversationItem is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"No conversation item with Id={ conversationId } found.");

            // TODO: parameters missing.
            return new IssueConversationDTO(conversationItem, null, null);
        }

        /// <summary>
        /// Returns a list of conversation items of the specified issue.
        /// </summary>
        /// <param name="issueId">The issue where the conversation items will be extracted from.</param>
        /// <returns>A list of conversation items from the provided issue.</returns>
        public async Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            if (ObjectId.TryParse(issueId, out ObjectId issueOid) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Cannot parse issue string id to a valid object id.");

            // fetch the issue that contains the conversation.
            Issue issue = await _issueRepository.GetAsync(issueOid);

            // show error if issue is not found.
            if (issue is null)
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"No Issue found with Id={ issueId }.");

            // if property is null, set default value => empty array of conversations which do not be saved to the database.
            if (issue.ConversationItems is null)
                issue.ConversationItems = new List<IssueConversation>();

            // get all conversations from the issue, sorted after the creation date.
            IList<IssueConversation> issueConversations = issue.ConversationItems.OrderBy(ci => ci.CreatedAt).ToList();

            // return empty list if null
            if (issueConversations is null)
                return new List<IssueConversationDTO>();

            // TODO: parameters missing.
            // map poco to dto.
            var issueConversationsDTOs = issueConversations.Select(ci => new IssueConversationDTO(ci, null, null)).ToList();

            // return the mapped conversation items.
            return issueConversationsDTOs;
        }
    }
}
