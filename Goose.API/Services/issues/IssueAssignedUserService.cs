using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IProjectRepository _projectRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IssueAssignedUserService(IIssueRepository issueRepo, IUserRepository userRepo, IProjectRepository projectRepository,
            IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _projectRepository = projectRepository;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
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
            return !issue.AssignedUserIds.Contains(userId) ? null : new UserDTO(await _userRepo.GetAsync(userId));
        }

        public async Task AssignUserAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            await CanAssign(issue);
            if (!issue.AssignedUserIds.Contains(userId))
                issue.AssignedUserIds.Add(userId);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task UnassignUserAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            await CanAssign(issue);
            if (issue.AssignedUserIds.Remove(userId))
                await _issueRepo.UpdateAsync(issue);
        }

        private async Task CanAssign(Issue issue)
        {
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            Dictionary<IAuthorizationRequirement, string> req = new()
            {
                {ProjectRolesRequirement.LeaderRequirement, "Your are not allowed to assign or unassign users."},
                {CompanyRolesRequirement.CompanyOwner, "Your are not allowed to assign or unassign users."}
            };
            (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, req.Keys)).ThrowErrorIfAllFailed(req);
        }
    }
}