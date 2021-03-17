using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.Models.identity;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace Goose.API.Services
{
    public interface IIssueAssignedUserService
    {
        public Task<IList<User>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<User>? GetAssignedUserOfIssueAsync(ObjectId issueId, ObjectId userId);
        Task AssignUserAsync(ObjectId issueId, ObjectId userId);
        Task UnassignUserAsync(ObjectId issueId, ObjectId userId);
    }

    public class IssueAssignedUserService : IIssueAssignedUserService
    {
        private readonly IIssueRepository _issueRepo;
        private readonly IUserRepository _userRepo;

        public IssueAssignedUserService(IIssueRepository issueRepo, IUserRepository userRepo)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
        }

        public async Task<IList<User>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var users = issue.AssignedUserIds.Select(it => _userRepo.GetAsync(it));
            return await Task.WhenAll(users);
        }

        public async Task<User>? GetAssignedUserOfIssueAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            if (!issue.AssignedUserIds.Contains(userId)) return null;
            return await _userRepo.GetAsync(userId);
        }

        public async Task AssignUserAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            if (!issue.AssignedUserIds.Contains(userId))
                issue.AssignedUserIds.Add(userId);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task UnassignUserAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            issue.AssignedUserIds.Remove(userId);
            await _issueRepo.UpdateAsync(issue);
        }
    }
}