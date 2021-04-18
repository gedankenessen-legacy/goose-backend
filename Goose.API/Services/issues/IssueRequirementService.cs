using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssueRequirementService
    {
        public Task<IList<IssueRequirement>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueRequirement> GetAsync(ObjectId issueId, ObjectId requirementId);
        public Task<IssueRequirement> CreateAsync(ObjectId issueId, IssueRequirement requirement);
        public Task UpdateAsync(ObjectId issueId, IssueRequirement requirement);
        public Task DeleteAsync(ObjectId issueId, ObjectId requirementId);
    }

    public class IssueRequirementService : IIssueRequirementService
    {
        private readonly IIssueRepository _issueRepo;

        public IssueRequirementService(IIssueRepository issueRepo)
        {
            _issueRepo = issueRepo;
        }

        public async Task<IList<IssueRequirement>> GetAllOfIssueAsync(ObjectId issueId)
        {
            return (await _issueRepo.GetAsync(issueId)).IssueDetail.Requirements ?? throw new HttpStatusException(400, "Das angefragte Ticket existiert nicht"); 
        }

        public async Task<IssueRequirement> GetAsync(ObjectId issueId, ObjectId requirementId)
        {
            return (await _issueRepo.GetAsync(issueId)).IssueDetail.Requirements.First(
                it => it.Id.Equals(requirementId)) ?? throw new HttpStatusException(400, "Das angefragte Ticket existiert nicht");
        }

        public async Task<IssueRequirement> CreateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            if(issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if(issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400, "Eine Anforderung für dieses Ticket konnte nicht erstellt werden, da schon eine Zusammenfassung erstellt wurde");

            if(string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine Valide Anforderung an");

            if (issue.IssueDetail.Requirements is null)
                issue.IssueDetail.Requirements = new List<IssueRequirement>();

            requirement.Id = ObjectId.GenerateNewId();
            issue.IssueDetail.Requirements.Add(requirement);
            await _issueRepo.UpdateAsync(issue);

            return requirement;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400, "Die Anforderung für dieses Ticket konnte nicht aktualliesiert werden, da schon eine Zusammenfassung erstellt wurde");

            if (string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine Valide Anforderung an");

            var req = issue.IssueDetail.Requirements.First(it => it.Id == requirement.Id);
            SetRequirementFields(req, requirement);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId requirementId)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400, "Die Anforderung für dieses Ticket konnte nicht entfernt werden, da schon eine Zusammenfassung erstellt wurde");

            var req = issue.IssueDetail.Requirements.First(it => it.Id.Equals(requirementId));
            issue.IssueDetail.Requirements.Remove(req);
            await _issueRepo.UpdateAsync(issue);
        }


        private void SetRequirementFields(IssueRequirement dest, IssueRequirement source)
        {
            dest.Requirement = source.Requirement;
        }
    }
}