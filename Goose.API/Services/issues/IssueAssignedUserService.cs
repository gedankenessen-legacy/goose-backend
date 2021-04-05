using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

#nullable enable

namespace Goose.API.Services.Issues
{
    public interface IIssueAssignedUserService
    {
        public Task<IList<UserDTO>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<UserDTO>? GetAssignedUserOfIssueAsync(ObjectId issueId, ObjectId userId);
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

        public async Task<IList<UserDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var users = issue.AssignedUserIds.Select(it => _userRepo.GetAsync(it));
            return (await Task.WhenAll(users)).Select(it => new UserDTO(it)).ToList();
        }

        public async Task<UserDTO>? GetAssignedUserOfIssueAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            if (!issue.AssignedUserIds.Contains(userId)) return null;
            return new UserDTO(await _userRepo.GetAsync(userId));
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