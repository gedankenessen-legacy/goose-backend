using System.Collections.Generic;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueParentService
    {
        public Task<IssueDTO?> GetParent(ObjectId issueId);
        public Task SetParent(ObjectId issueId, ObjectId parentId);
        public Task RemoveParent(ObjectId issueId);
    }

    public class IssueParentService : IIssueParentService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IIssueService _issueService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStateService _stateService;

        public IssueParentService(IIssueService issueService, IIssueRepository issueRepository, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor, IStateService stateService)
        {
            _issueService = issueService;
            _issueRepository = issueRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _stateService = stateService;
        }

        public async Task<IssueDTO>? GetParent(ObjectId issueId)
        {
            var parentId = (await _issueRepository.GetAsync(issueId)).ParentIssueId;
            if (parentId == null) return null;
            return await _issueService.Get((ObjectId) parentId);
        }

        public async Task SetParent(ObjectId issueId, ObjectId parentId)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            var parent = await _issueRepository.GetAsync(parentId);

            await UserCanAddParent(issue);

            if (issue.ProjectId != parent.ProjectId)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Issues müssen im selben Projekt sein");
            var parentState = await _stateService.GetState(parent.ProjectId, parent.Id);

            if (parentState.Phase == State.ConclusionPhase || parentState.Name == State.ReviewState)
                throw new HttpStatusException(StatusCodes.Status400BadRequest,
                    "An issue cannot be set as a child if it's being reviewed or in the conclusion phase");

            issue.ParentIssueId = parentId;
            parent.ChildrenIssueIds.Add(issueId);
            await Task.WhenAll(_issueRepository.UpdateAsync(issue), _issueRepository.UpdateAsync(parent));

            // ConversationItem im Oberticket hinzufügen
            parent.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.ChildIssueAddedType,
                Data = null,
                OtherTicketId = issueId,
            });
            await _issueRepository.UpdateAsync(parent);
        }

        private async Task UserCanAddParent(Issue issue)
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                {IssueOperationRequirments.AddSubTicket, "Your are not allowed to add a parent to this issue."}
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }

        public async Task RemoveParent(ObjectId issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            var mightBeParentId = issue.ParentIssueId;
            issue.ParentIssueId = null;
            await _issueRepository.UpdateAsync(issue);

            if (mightBeParentId is ObjectId parentId)
            {
                // ConversationItem im Oberticket hinzufügen
                var parent = await _issueRepository.GetAsync(parentId);
                parent.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.ChildIssueRemovedType,
                    Data = null,
                    OtherTicketId = issueId,
                });
                await _issueRepository.UpdateAsync(parent);
            }
        }
    }
}