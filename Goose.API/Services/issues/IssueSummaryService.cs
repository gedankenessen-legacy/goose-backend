using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services.issues
{
    public interface IIssueSummaryService
    {
        public Task<IList<IssueRequirement>> CreateSummary(string issueId);
        public Task<IList<IssueRequirement>> GetSummary(string issueId);
        public Task AcceptSummary(string issueId);
        public Task DeclineSummary(string issueId);
    }

    public class IssueSummaryService : IIssueSummaryService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IIssueRequirementService _issueRequirementService;
        private readonly IStateService _stateService;

        public IssueSummaryService(IIssueRepository issueRepository, IIssueRequirementService issueRequirementService, IStateService stateService)
        {
            _issueRepository = issueRepository;
            _issueRequirementService = issueRequirementService;
            _stateService = stateService;
        }

        public async Task AcceptSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if(issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Es wurde noch kein Zusammenfassung für dieses Ticket erstellt");

            issue.IssueDetail.RequirementsAccepted = true;
            var states = await _stateService.GetStates(issue.ProjectId);

            if(states is null)
                throw new HttpStatusException(400, "Es wurden keine Statuse für dieses Project gefunden");

            var state = states.FirstOrDefault(x => x.Name.Equals(State.WaitingState));

            if(state is null)
                throw new HttpStatusException(400, "Es wurde kein State gefunden");

            issue.IssueDetail.RequirementsAccepted = true;
            issue.StateId = state.Id;
            await _issueRepository.UpdateAsync(issue);
        }

        public async Task<IList<IssueRequirement>> CreateSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if(issue.IssueDetail.Requirements is null)
                throw new HttpStatusException(400, "Die Requirements waren null");

            if(issue.IssueDetail.Requirements.Count <= 0 && issue.IssueDetail.ExpectedTime <= 0)
                throw new HttpStatusException(400, "Um eine Zusammenfassung erstellen zu können muss mindestens eine Anforderung oder eine geschätze Zeit vorhanden sein");

            issue.IssueDetail.RequirementsSummaryCreated = true;
            await _issueRepository.UpdateAsync(issue);

            return await _issueRequirementService.GetAllOfIssueAsync(issueId.ToObjectId());
        }

        public async Task DeclineSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if(issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Es wurde noch keine Zusammenfassung erstellt und kann deswegen nicht abgelehnt werden");

            if (issue.IssueDetail.RequirementsAccepted)
                throw new HttpStatusException(400, "Die Zusammenfassung wurde schon angenommen und kann nicht abgelehnt werden");

            issue.IssueDetail.RequirementsSummaryCreated = false;
            await _issueRepository.UpdateAsync(issue);
        }

        public async Task<IList<IssueRequirement>> GetSummary(string issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId.ToObjectId());

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if(issue.IssueDetail.RequirementsSummaryCreated is false)
                throw new HttpStatusException(400, "Die Zusammenfassung wurde noch nicht erstellt");

            return await _issueRequirementService.GetAllOfIssueAsync(issueId.ToObjectId());
        }
    }
}
