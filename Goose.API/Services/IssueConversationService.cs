using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.API.Utils.Validators;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
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
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageService _messageService;

        public IssueConversationService(
            IIssueRepository issueRepository,
            IIssueService issueService,
            IUserService userService,
            IProjectRepository projectRepository,
            IRoleRepository roleRepository,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IMessageService messageService)
        {
            _issueRepository = issueRepository;
            _issueService = issueService;
            _userService = userService;
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _messageService = messageService;
        }

        /// <summary>
        /// Returns a list of conversation items of the specified issue.
        /// </summary>
        /// <param name="issueId">The issue where the conversation items will be extracted from.</param>
        /// <returns>A list of conversation items from the provided issue.</returns>
        public async Task<IList<IssueConversationDTO>> GetConversationsFromIssueAsync(string issueId)
        {
            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            await AssertUserCanReadConversation(issue);
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

            if (ic.OtherTicketId is ObjectId otherTicketId) {
                var otherIssue = await _issueRepository.GetAsync(otherTicketId);
                var otherIssueName = otherIssue.IssueDetail.Name;

                ic.Data = ic.Type switch
                {
                    IssueConversation.PredecessorAddedType => $"{otherIssueName} wurde als Vorgänger hinzugefügt.",
                    IssueConversation.PredecessorRemovedType => $"{otherIssueName} wurde als Vorgänger entfernt.",
                    IssueConversation.ChildIssueAddedType => $"{otherIssueName} wurde als Unterticket hinzugefügt.",
                    IssueConversation.ChildIssueRemovedType => $"{otherIssueName} wurde als Unterticket entfernt.",
                    _ => throw new HttpStatusException(StatusCodes.Status500InternalServerError, "Invalid conversation item")
                };
            }

            return new IssueConversationDTO(ic, creator);
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
            await AssertUserCanReadConversation(issue);

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
            await AssertUserCanWriteConversation(issue);

            await _issueService.AssertNotArchived(issue);

            var conversationItems = issue.ConversationItems;

            // if ConversationItems are null = empty, create a new list, which will gets appended.
            if (conversationItems is null)
                conversationItems = new List<IssueConversation>();

            // TODO: after auth code impl. check if the user who creating this request is identical to the "conversationItem.Creator.Id" -> a client cannot create a conversation for other than himself.

            IssueConversation newConversation = new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Data = conversationItem.Data,
                Requirements = new List<string>(),
                Type = conversationItem.Type
            };

            // append the conversationItems with the new conversation item.
            conversationItems.Add(newConversation);

            // send Message to Author
            await CreateMessageForAuthor(issue);

            // update the issue and the conversationItems withit.
            await _issueRepository.UpdateAsync(issue);

            return await MapIssueConversationDTOAsync(issue.Id, newConversation);
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

            var issue = await _issueRepository.GetIssueByIdAsync(issueId);
            await AssertUserCanWriteConversation(issue);

            if (conversationItemOid.Equals(conversationItem.Id) is false)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Id missmatch.");

            var issueConversationModel = new IssueConversation()
            {
                Id = conversationItem.Id,
                CreatorUserId = Validators.ValidateObjectId(conversationItem.Creator?.Id.ToString(), "The conversation item is missing a valid userId."),
                Data = conversationItem.Data,
                // Conversation from the outside  is always a message and
                // can therefore not contain requirements
                Requirements = null,
                Type = IssueConversation.MessageType,
            };

            await _issueRepository.CreateOrUpdateConversationItemAsync(issueId, issueConversationModel);
        }

        private async Task AssertUserCanWriteConversation(Issue issue)
        {

            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { IssueOperationRequirments.WriteMessage, "Your are not allowed to write a message." }
            };

            // validate requirements with the appropriate handlers.
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }

        private async Task AssertUserCanReadConversation(Issue issue)
        {

            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { IssueOperationRequirments.ReadMessages, "Your are not allowed to read a message." }
            };

            // validate requirements with the appropriate handlers.
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorIfAllFailed(requirementsWithErrors);
            
        }

        private async Task CreateMessageForAuthor(Issue issue)
        {
            var project = await _projectRepository.GetAsync(issue.ProjectId);

            await _messageService.CreateMessageAsync(new Message()
            {
                CompanyId = project.CompanyId,
                ProjectId = project.Id,
                IssueId = issue.Id,
                ReceiverUserId = issue.AuthorId,
                Type = MessageType.NewConversationItem,
                Consented = false,
            });
        }
    }
}
