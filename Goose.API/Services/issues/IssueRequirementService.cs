using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssueRequirementService
    {
        public Task<IList<IssueRequirement>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueRequirement> GetAsync(ObjectId issueId, ObjectId requirementId);
        public Task<IssueRequirement> CreateAsync(ObjectId issueId, IssueRequirement requirement);
        public Task UpdateAsync(ObjectId issueId, ObjectId requirementId, IssueRequirement requirement);
        public Task DeleteAsync(ObjectId issueId, ObjectId requirementId);
    }

    public class IssueRequirementService : AuthorizableService, IIssueRequirementService
    {
        private readonly IIssueRepository _issueRepo;

        public IssueRequirementService(IIssueRepository issueRepo, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor, authorizationService)
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

            await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.CreateRequirements);

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400, "Eine Anforderung für dieses Ticket konnte nicht erstellt werden, da schon eine Zusammenfassung erstellt wurde");

            if (string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine Valide Anforderung an");

            if (issue.IssueDetail.Requirements is null)
                issue.IssueDetail.Requirements = new List<IssueRequirement>();

            requirement.Id = ObjectId.GenerateNewId();
            issue.IssueDetail.Requirements.Add(requirement);
            await _issueRepo.UpdateAsync(issue);

            return requirement;
        }

        public async Task UpdateAsync(ObjectId issueId, ObjectId requirementId, IssueRequirement requirement)
        {
            if (requirementId.Equals(requirement.Id) is false)
                throw new HttpStatusException(400, "Die ID des Pfades passt nicht zu der ID der Ressource.");

            var issue = await _issueRepo.GetAsync(issueId);

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine valide Anforderung an");

            var req = issue.IssueDetail.Requirements.First(it => it.Id == requirement.Id);
            await SetRequirementFieldsAsync(issue, req, requirement);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId requirementId)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.RemoveRequirements);

            if (issue is null)
                throw new HttpStatusException(400, "Das angefragte Issue Existiert nicht");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400, "Die Anforderung für dieses Ticket konnte nicht entfernt werden, da schon eine Zusammenfassung erstellt wurde");

            var req = issue.IssueDetail.Requirements.First(it => it.Id.Equals(requirementId));
            issue.IssueDetail.Requirements.Remove(req);
            await _issueRepo.UpdateAsync(issue);
        }


        private async Task SetRequirementFieldsAsync(Issue issue, IssueRequirement dest, IssueRequirement source)
        {
            if (dest.Requirement != source.Requirement)
            {
                if (issue.IssueDetail.RequirementsSummaryCreated)
                    throw new HttpStatusException(400, "Die Anforderung für dieses Ticket konnte nicht aktualisiert werden, da schon eine Zusammenfassung erstellt wurde");
                else
                    await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.EditRequirements);
            }

            if (dest.Achieved != source.Achieved)
                await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.AchieveRequirements, "Du hast nicht die Rechte um ein Requirment als abgeschlossen zu markieren");

            dest.Requirement = source.Requirement;
            dest.Achieved = source.Achieved;
        }
    }
}