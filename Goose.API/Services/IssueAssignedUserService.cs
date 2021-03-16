using System.Threading.Tasks;
using Goose.API.Repositories;
using MongoDB.Bson;

namespace Goose.API.Services
{
    public interface IIssueAssignedUserService
    {
        Task AssignUser(ObjectId issueId, ObjectId userId);
        Task UnassignUser(ObjectId issueId, ObjectId userId);
    }

    public class IssueAssignedUserService: IIssueAssignedUserService
    {
        private readonly IIssueRepository _issueRepo;

        public IssueAssignedUserService(IIssueRepository issueRepo)
        {
            _issueRepo = issueRepo;
        }

        public async Task AssignUser(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            if (!issue.AssignedUserIds.Contains(userId))
                issue.AssignedUserIds.Add(userId);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task UnassignUser(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            issue.AssignedUserIds.Remove(userId);
            await _issueRepo.UpdateAsync(issue);
        }
    }
}