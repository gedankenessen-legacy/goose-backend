using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using Goose.API.Repositories;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.API.Services.issues
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
            return (await _issueRepo.GetAsync(issueId)).IssueDetail.Requirements;
        }

        public async Task<IssueRequirement> GetAsync(ObjectId issueId, ObjectId requirementId)
        {
            return (await _issueRepo.GetAsync(issueId)).IssueDetail.Requirements.First(
                it => it.Id.Equals(requirementId));
        }

        public async Task<IssueRequirement> CreateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            requirement.Id = ObjectId.GenerateNewId();
            
            var issue = await _issueRepo.GetAsync(issueId);
            issue.IssueDetail.Requirements.Add(requirement);
            await _issueRepo.UpdateAsync(issue);
            return requirement;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var req = issue.IssueDetail.Requirements.First(it => it.Id == requirement.Id);
            SetRequirementFields(req, requirement);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId requirementId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
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