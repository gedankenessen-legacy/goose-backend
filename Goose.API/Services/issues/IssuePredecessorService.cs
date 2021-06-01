using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Services.issues;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssuePredecessorService
    {
        public Task<IList<IssueDTO>> GetAll(ObjectId issueId);
        public Task SetPredecessor(ObjectId successorId, ObjectId predecessorId);
        public Task RemovePredecessor(ObjectId successorId, ObjectId predecessorId);
    }

    public class IssuePredecessorService : IIssuePredecessorService
    {
        private readonly IIssueRepository _issueRepo;
        private readonly IIssueService _issueService;
        private readonly IIssueAssociationHelper _associationHelper;
        private readonly IStateService _stateService;
        private readonly IProjectRepository _projectRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IssuePredecessorService(IIssueRepository issueRepo, IIssueService issueService, IHttpContextAccessor httpContextAccessor,
            IIssueAssociationHelper associationHelper, IStateService stateService, IProjectRepository projectRepository,
            IAuthorizationService authorizationService)
        {
            _issueRepo = issueRepo;
            _httpContextAccessor = httpContextAccessor;
            _associationHelper = associationHelper;
            _stateService = stateService;
            _projectRepository = projectRepository;
            _authorizationService = authorizationService;
            _issueService = issueService;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            return await Task.WhenAll(issue.PredecessorIssueIds.Select(it => _issueService.Get(it)));
        }

        public async Task SetPredecessor(ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetAsync(successorId);
            var predecessor = await _issueRepo.GetAsync(predecessorId);

            if (successor.ProjectId != predecessor.ProjectId)
                throw new HttpStatusException(400, "cannot add a predecessor from another project");
            await CanEditPredecessor(successor.ProjectId);

            if (!predecessor.IssueDetail.Visibility && successor.IssueDetail.Visibility)
                throw new HttpStatusException(400, "An intern issue cannot be the predecessor of an extern issue");
            var predecessorState = await _stateService.GetState(predecessor.ProjectId, predecessor.StateId);
            if (predecessorState.Phase == State.ConclusionPhase)
                throw new HttpStatusException(400, "A predecessor cannot be in conclusion phase already");
            if (predecessor.IssueDetail.StartDate is { } predecessorStartDate)
                if (successor.IssueDetail.EndDate is { } successorEndDate)
                    if (predecessorStartDate >= successorEndDate)
                        throw new HttpStatusException(400, "The start date of a predecessor cannot be before the end date of the successor");
            await _associationHelper.CanAddPredecessor(successor, predecessor);

            successor.PredecessorIssueIds.Add(predecessorId);
            predecessor.SuccessorIssueIds.Add(successorId);

            successor.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.PredecessorAddedType,
                Data = null,
                OtherTicketId = predecessorId,
            });

            await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
        }

        public async Task RemovePredecessor(ObjectId successorId, ObjectId predecessorId)
        {
            var successor = await _issueRepo.GetAsync(successorId);
            var predecessor = await _issueRepo.GetAsync(predecessorId);

            if (successor.ProjectId != predecessor.ProjectId)
                throw new HttpStatusException(400, "cannot add a predecessor from another project");
            await CanEditPredecessor(successor.ProjectId);

            if (successor.PredecessorIssueIds.Remove(predecessorId) ||
                predecessor.SuccessorIssueIds.Remove(successorId))
            {
                successor.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.PredecessorRemovedType,
                    Data = null,
                    OtherTicketId = predecessorId,
                });

                await Task.WhenAll(_issueRepo.UpdateAsync(successor), _issueRepo.UpdateAsync(predecessor));
            }
        }

        private async Task CanEditPredecessor(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                {ProjectRolesRequirement.LeaderRequirement, "Your are not allowed to add a predecessor."},
                {ProjectRolesRequirement.EmployeeRequirement, "Your are not allowed to add a predecessor."},
                {CompanyRolesRequirement.CompanyOwner, "Your are not allowed to add a predecessor."}
            };
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorIfAllFailed(requirementsWithErrors);
        }
    }
}