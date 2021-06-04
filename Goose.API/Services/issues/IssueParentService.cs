using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils;
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
        private readonly IIssueHelper _issueHelper;

        public IssueParentService(IIssueService issueService, IIssueRepository issueRepository, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor, IStateService stateService, IIssueHelper _issueHelper)
        {
            _issueService = issueService;
            _issueRepository = issueRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _stateService = stateService;
            this._issueHelper = _issueHelper;
        }

        public async Task<IssueDTO?> GetParent(ObjectId issueId)
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

            var parentState = await _stateService.GetState(parent.ProjectId, parent.StateId);
            if (parentState.Phase == State.ConclusionPhase || parentState.Name == State.ReviewState)
                throw new HttpStatusException(StatusCodes.Status400BadRequest,
                    "An issue cannot be set as a child if it's being reviewed or in the conclusion phase");

            if (issue.IssueDetail.Visibility != parent.IssueDetail.Visibility)
            {
                throw new HttpStatusException(StatusCodes.Status400BadRequest,
                    "The visibility Status of Parent and Child must be the same");
            }

            await _issueHelper.CanAddChild(parent, issue);

            issue.ParentIssueId = parentId;
            parent.ChildrenIssueIds.Add(issueId);
            if(parentState.Phase == State.ProcessingPhase)
                parent.StateId = (await _stateService.GetStates(parent.ProjectId)).First(it => it.Name == State.BlockedState).Id;
                
            // ConversationItem im Oberticket hinzufügen
            parent.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.ChildIssueAddedType,
                Data = null,
                OtherTicketId = issueId,
            });
            await Task.WhenAll(_issueRepository.UpdateAsync(issue), _issueRepository.UpdateAsync(parent));
            await _issueService.PropagateDependentProperties(parent);
        }

        private async Task UserCanAddParent(Issue issue)
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                {IssueOperationRequirments.AddSubIssue, "Your are not allowed to add a parent to this issue."}
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }
        public async Task RemoveParent(ObjectId issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            if (issue.ParentIssueId is { } parentId)
            {
                var parent = await _issueRepository.GetAsync(parentId);
                issue.ParentIssueId = null;
                parent.ChildrenIssueIds.Remove(it => it == issue.Id);
                // ConversationItem im Oberticket hinzufügen
                
                parent.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.ChildIssueRemovedType,
                    Data = null,
                    OtherTicketId = issueId,
                });
                
                await Task.WhenAll(_issueRepository.UpdateAsync(issue), _issueRepository.UpdateAsync(parent));
            }
        }
    }
}