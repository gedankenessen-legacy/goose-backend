using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using Microsoft.AspNetCore.Http;
using Goose.API.Utils.Authentication;
using Microsoft.AspNetCore.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Authorization;
using Goose.Domain.Models.Identity;

namespace Goose.API.Services.issues
{
    public interface IIssueSummaryService
    {
        public Task<IList<IssueRequirement>> CreateSummary(string issueId, double expectedTime);
        public Task<IList<IssueRequirement>> GetSummary(string issueId);
        public Task AcceptSummary(string issueId);
        public Task DeclineSummary(string issueId);
    }

    public class IssueSummaryService : IIssueSummaryService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRequirementService _issueRequirementService;
        private readonly IStateService _stateService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIssueStateService _issueStateService;

        public IssueSummaryService(
            IIssueRepository issueRepository,
            IIssueRequirementService issueRequirementService,
            IStateService stateService,
            IHttpContextAccessor httpContextAccessor,
            IProjectRepository projectRepository,
            IAuthorizationService authorizationService, IIssueStateService issueStateService)
        {
            _issueRepository = issueRepository;
            _issueRequirementService = issueRequirementService;
            _stateService = stateService;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _issueStateService = issueStateService;
            _projectRepository = projectRepository;
        }

        public async Task AcceptSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (!(await CanUserAcceptOrDeclineSummary(issue)))
                throw new HttpStatusException(400, "Sie haben nicht die berechtigung eine Zusammenfassung zu akzeptieren");

            if (issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Es wurde noch kein Zusammenfassung für dieses Ticket erstellt");

            issue.IssueDetail.RequirementsAccepted = true;
            var states = await _stateService.GetStates(issue.ProjectId);

            if (states is null)
                throw new HttpStatusException(400, "Es wurden keine Statuse für dieses Project gefunden");

            var state = await _issueStateService.UpdateState(issue, states.First(it => it.Name == State.ProcessingState));
            issue = await _issueRepository.GetAsync(issueId.ToObjectId());
            
            if (state is null)
                throw new HttpStatusException(400, "Es wurde kein State gefunden");

            issue.IssueDetail.RequirementsAccepted = true;
            issue.StateId = state.Id;

            issue.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.SummaryAcceptedType,
                Data = "",
                Requirements = issue.IssueDetail.Requirements.Select(x => x.Requirement).ToList(),
                ExpectedTime = issue.IssueDetail.ExpectedTime
            });

            issue.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.StateChangeType,
                Data = $"Status von {State.NegotiationState} zu {state.Name} geändert.",
                StateChange = new()
                {
                    Before = State.NegotiationState,
                    After = state.Name
                }
            });
            await _issueRepository.UpdateAsync(issue);
        }

        public async Task<IList<IssueRequirement>> CreateSummary(string issueId, double expectedTime)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            await SummaryLeaderRight(issue);

            if (issue.IssueDetail.Requirements is null)
                throw new HttpStatusException(400, "Die Requirements waren null");

            if (issue.IssueDetail.Requirements.Count <= 0 && issue.IssueDetail.ExpectedTime <= 0)
                throw new HttpStatusException(400,
                    "Um eine Zusammenfassung erstellen zu können muss mindestens eine Anforderung oder eine geschätze Zeit vorhanden sein");

            issue.IssueDetail.RequirementsSummaryCreated = true;
            issue.IssueDetail.ExpectedTime = expectedTime;
            issue.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.SummaryCreatedType,
                Data = "",
                Requirements = issue.IssueDetail.Requirements.Select(x => x.Requirement).ToList(),
                ExpectedTime = issue.IssueDetail.ExpectedTime
            });
            await _issueRepository.UpdateAsync(issue);

            return await _issueRequirementService.GetAllOfIssueAsync(issueId.ToObjectId());
        }

        public async Task DeclineSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (!(await CanUserAcceptOrDeclineSummary(issue)))
                throw new HttpStatusException(400, "Sie haben nicht die berechtigung eine Zusammenfassung abzulehnen");

            if (issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Es wurde noch keine Zusammenfassung erstellt und kann deswegen nicht abgelehnt werden");

            if (issue.IssueDetail.RequirementsAccepted)
                throw new HttpStatusException(400, "Die Zusammenfassung wurde schon angenommen und kann nicht abgelehnt werden");

            issue.IssueDetail.RequirementsSummaryCreated = false;
            issue.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.SummaryDeclinedType,
                Data = "",
                Requirements = issue.IssueDetail.Requirements.Select(x => x.Requirement).ToList(),
                ExpectedTime = issue.IssueDetail.ExpectedTime
            });
            await _issueRepository.UpdateAsync(issue);
        }

        public async Task<IList<IssueRequirement>> GetSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Die Zusammenfassung wurde noch nicht erstellt");

            return await _issueRequirementService.GetAllOfIssueAsync(issueId.ToObjectId());
        }

        private async Task SummaryLeaderRight(Issue issue)
        {
            var project = await _projectRepository.GetAsync(issue.ProjectId) ??
                          throw new HttpStatusException(400, $"Es Existiert kein Project mit der ID {issue.ProjectId}");
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                {CompanyRolesRequirement.CompanyOwner, "You need to be the owner of this company, in order to create, accept or decline a Summary."},
                {ProjectRolesRequirement.LeaderRequirement, "You need to be the Leader of this Project, in order to create, accept or decline a Summary."}
            };

            (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, requirementsWithErrors.Keys)).ThrowErrorIfAllFailed(
                requirementsWithErrors);
        }


        public async Task<bool> IsUserTheProjectLeaderOrCompanyOwner(Project project)
        {
            IList<IAuthorizationRequirement> requirements = new List<IAuthorizationRequirement>()
            {
                ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner
            };

            // validate requirements with the appropriate handlers.
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, requirements);

            return authorizationResult.Failure.FailedRequirements.Count() < requirements.Count;
        }

        public async Task<bool> CanUserAcceptOrDeclineSummary(Issue issue)
        {
            //Get Project 
            var project = await _projectRepository.GetAsync(issue.ProjectId)
                          ?? throw new HttpStatusException(400, $"Es Existiert kein Project mit der ID {issue.ProjectId}");

            // Check is Author a customer?
            var user = project.Users.FirstOrDefault(x => x.UserId.Equals(issue.AuthorId));

            if (user is null)
                throw new HttpStatusException(400, $"something went wrong");

            var isCustomer = user.RoleIds.Any(x => x.Equals(Role.CustomerRole.Id));

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

            if (issue.AuthorId.Equals(userId))
                return true;

            // if no only Author, Projectleader or Company can accept or decline 
            if (!isCustomer)
                return await IsUserTheProjectLeaderOrCompanyOwner(project);

            return false;
        }
    }
}