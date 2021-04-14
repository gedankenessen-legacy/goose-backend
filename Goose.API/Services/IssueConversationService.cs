using Goose.API.Repositories;
using Goose.API.Services.issues;
using Goose.API.Utils.Exceptions;
using Goose.API.Utils.Validators;
using Goose.Domain.DTOs.Issues;
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
        public Task<IssueConversationDTO> CreateNewIssueConversationAsync(string issueId, IssueConversationDTO conversationItem);
        public Task CreateOrReplaceConversationItemAsync(string issueId, string conversationItemId, IssueConversationDTO conversationItem);
    }

    public class IssueConversationService : IIssueConversationService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IIssueService _issueService;
        private readonly IUserService _userService;

        public IssueConversationService(IIssueRepository issueRepository, IIssueService issueService, IUserService userService)
        {
            _issueRepository = issueRepository;
            _issueService = issueService;
            _userService = userService;
        }

        /// <summary>
        /// Returns a list of conversation items of the specified issue.
        /// </summary>
        /// <param name="issueId">The issue where the conversation items will be extracted from.</param>
        /// <returns>A list of conversation items from the provided issue.</returns>
        public async Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId)
        {
            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            var conversationItems = issue.ConversationItems;

            // if property is null, set default value => empty array of conversations which do not be saved to the database.
            if (conversationItems is null)
                return new List<IssueConversationDTO>();

            // get all conversations from the issue, sorted after the creation date.
            IList<IssueConversation> issueConversations = conversationItems.OrderBy(ci => ci.CreatedAt).ToList();

            // map poco to dto.
            var issueConversationsDTOs = await Task.WhenAll(issueConversations.Select(async ic => await MapIssueConversationDTOAsync(issue.Id, ic)).ToList());

            // return the mapped conversation items.
            return issueConversationsDTOs;
        }

        private async Task<IssueConversationDTO> MapIssueConversationDTOAsync(ObjectId issueId, IssueConversation ic)
        {
            var creator = await _userService.GetUser(ic.CreatorUserId);

            var requirements = await Task.WhenAll(ic.RequirementIds.Select(async reqOid => await _issueRepository.GetRequirementByIdAsync(issueId, reqOid)).ToList());

            return new IssueConversationDTO(ic, creator, requirements);
        }

        /// <summary>
        /// Returns the requested conversation for the provided issueId.
        /// </summary>
        /// <param name="issueId">The Issue in which the conversation id will be searched for.</param>
        /// <param name="conversationId">The Id of the particular conversation.</param>
        /// <returns>A conversationDTO for the provided issue und conversation id</returns>
        public async Task<IssueConversationDTO> GetConversationFromIssueAsync(string issueId, string conversationId)
        {
            // check if the parsed objectId is not the 000...000 default objectId.
            ObjectId conversationOid = Validators.ValidateObjectId(conversationId, "Cannot parse conversation string id to a valid object id.");


            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            var conversationItems = issue.ConversationItems;

            if (conversationItems is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "No conversation items found.");

            var conversationItem = conversationItems.SingleOrDefault(ci => ci.Id.Equals(conversationOid));

            if (conversationItem is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"No conversation item with Id={ conversationId } found.");

            return await MapIssueConversationDTOAsync(issue.Id, conversationItem);
        }

        /// <summary>
        /// Creates a new conversation for the provided issueId.
        /// </summary>
        /// <param name="issueId">The issue which will gets appended with the provided conversation.</param>
        /// <param name="conversationItem">The new conversation item.</param>
        /// <returns>The inserted conversation item.</returns>
        public async Task<IssueConversationDTO> CreateNewIssueConversationAsync(string issueId, IssueConversationDTO conversationItem)
        {
            var issue = await _issueRepository.GetIssueByIdAsync(issueId);

            await _issueService.AssertNotArchived(issue);

            var conversationItems = issue.ConversationItems;

            // if ConversationItems are null = empty, create a new list, which will gets appended.
            if (conversationItems is null)
                conversationItems = new List<IssueConversation>();

            // TODO: after auth code impl. check if the user who creating this request is identical to the "conversationItem.Creator.Id" -> a client cannot create a conversation for other than himself.

            IssueConversation newConversation = new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = conversationItem.Creator.Id,
                Data = conversationItem.Data,
                RequirementIds = SelectRequirementsObjectIdsFromDto(conversationItem),
                Type = conversationItem.Type
            };

            // append the conversationItems with the new conversation item.
            conversationItems.Add(newConversation);

            // update the issue and the conversationItems withit.
            await _issueRepository.UpdateAsync(issue);

            return await MapIssueConversationDTOAsync(issue.Id, newConversation);
        }

        /// <summary>
        /// This Method returns a list of ObjectIds from the Requirments.Id inside of the provided conversationItem.
        /// </summary>
        /// <param name="conversationItem">The conversation which contains the requirements.</param>
        /// <returns>A list of ObjectIds from the requierments.</returns>
        private IList<ObjectId> SelectRequirementsObjectIdsFromDto(IssueConversationDTO conversationItem)
        {
            if (conversationItem.Requirements is null)
                return new List<ObjectId>();

            return conversationItem.Requirements.Select(req => req.Id).ToList();
        }

        /// <summary>
        /// Creates or replaces the provided conversation item, based on the id.
        /// </summary>
        /// <param name="issueId">The issue on which the operation will be executed at.</param>
        /// <param name="conversationItemId">The id of the conversation.</param>
        /// <param name="conversationItem">The conversation that will be created or replaced if already existing.</param>
        public async Task CreateOrReplaceConversationItemAsync(string issueId, string conversationItemId, IssueConversationDTO conversationItem)
        {
            ObjectId conversationItemOid = Validators.ValidateObjectId(conversationItemId, "Provided conversation id is no valid ObjectId.");

            if (conversationItemOid.Equals(conversationItem.Id) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Id missmatch.");

            var issueConversationModel = new IssueConversation()
            {
                Id = conversationItem.Id,
                CreatorUserId = Validators.ValidateObjectId(conversationItem.Creator?.Id.ToString(), "The conversation item is missing a valid userId."),
                Data = conversationItem.Data,
                RequirementIds = SelectRequirementsObjectIdsFromDto(conversationItem),
                Type = conversationItem.Type
            };

            await _issueRepository.CreateOrUpdateConversationItemAsync(issueId, issueConversationModel);
        }
    }
}
