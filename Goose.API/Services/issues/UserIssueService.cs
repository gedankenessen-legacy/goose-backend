using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Services.Issues;
using Goose.API.Utils;
using Goose.Domain.DTOs.Issues;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IUserIssueService
    {
        public Task<IList<IssueDTO>> GetAllOfUser(ObjectId projectId, ObjectId userId);
    }

    public class UserIssueService : IUserIssueService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IIssueService _issueService;

        public UserIssueService(IIssueService issueService, IIssueRepository issueRepository)
        {
            _issueService = issueService;
            _issueRepository = issueRepository;
        }

        public async Task<IList<IssueDTO>> GetAllOfUser(ObjectId projectId, ObjectId userId)
        {
            var issuesOfProject = await _issueRepository.GetAllOfProjectAsync(projectId);
            var issuesOfUser = issuesOfProject.Where(it => it.AssignedUserIds.Contains(userId));
            return await issuesOfUser.Select(it => _issueService.Get(it.Id)).AwaitAll();
        }
    }
}