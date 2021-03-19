using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueRequirementService
    {
        public Task<IList<IssueRequirementDTO>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueRequirementDTO> GetAsync(ObjectId issueId, ObjectId requirementId);
        public Task<IssueRequirementDTO> CreateAsync(ObjectId issueId, IssueRequirementDTO requirement);
        public Task UpdateAsync(ObjectId issueId, IssueRequirementDTO requirement);
        public Task DeleteAsync(ObjectId issueId);
    }

    public class IssueRequirementService : IIssueRequirementService
    {
        private readonly IIssueRepository _issueRepo;
        private readonly IMapper _mapper;

        public IssueRequirementService(IIssueRepository issueRepo, IMapper mapper)
        {
            _issueRepo = issueRepo;
            _mapper = mapper;
        }

        public async Task<IList<IssueRequirementDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            return _mapper.Map<List<IssueRequirementDTO>>((await _issueRepo.GetAsync(issueId)).IssueDetail
                .Requirements);
        }

        public async Task<IssueRequirementDTO> GetAsync(ObjectId issueId, ObjectId requirementId)
        {
            return _mapper.Map<IssueRequirementDTO>((await _issueRepo.GetAsync(issueId)).IssueDetail
                .Requirements.First(it => it.Id.Equals(requirementId)));
        }

        public async Task<IssueRequirementDTO> CreateAsync(ObjectId issueId, IssueRequirementDTO requirement)
        {
            //TODO nicht atomar
            var req = await _issueRepo.GetAsync(issueId);
            req.IssueDetail.Requirements.Add(_mapper.Map<IssueRequirement>(requirement));
            await _issueRepo.UpdateAsync(req);
            return requirement;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueRequirementDTO requirement)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var req = issue.IssueDetail.Requirements.First(it => it.Id == requirement.Id);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId)
        {
            throw new System.NotImplementedException();
        }
    }
}