using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.DTOs.Issues;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssueSuccessorService
    {
        public Task<IList<IssueDTO>> GetAll(ObjectId issueId);
    }

    public class IssueSuccessorService : IIssueSuccessorService
    {
        private readonly IIssueRepository _issueRepo;
        private readonly IIssueService _issueService;

        public IssueSuccessorService(IIssueRepository issueRepo, IIssueService issueService)
        {
            _issueRepo = issueRepo;
            _issueService = issueService;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            return await Task.WhenAll(issue.SuccessorIssueIds.Select(it => _issueService.Get(it)));
        }
    }
}