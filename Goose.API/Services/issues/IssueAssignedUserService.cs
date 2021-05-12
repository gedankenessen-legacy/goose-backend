using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IAuthorizationService _authorizationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public IssueAssignedUserService(IIssueRepository issueRepo, IUserRepository userRepo, IAuthorizationService authorizationService,
            IProjectRepository projectRepository, IHttpContextAccessor contextAccessor)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _authorizationService = authorizationService;
            _projectRepository = projectRepository;
            _contextAccessor = contextAccessor;
        }

        public async Task<IList<UserDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            if (await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, project, ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner))
            {
                var users = issue.AssignedUserIds.Select(it => _userRepo.GetAsync(it));
                return (await Task.WhenAll(users)).Select(it => new UserDTO(it)).ToList();
            }

            throw new HttpStatusException(StatusCodes.Status403Forbidden, "Only Project leaders or company owners can access this resource");
        }

        public async Task<UserDTO>? GetAssignedUserOfIssueAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            if (await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, project, ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner))
            {
                return !issue.AssignedUserIds.Contains(userId) ? null : new UserDTO(await _userRepo.GetAsync(userId));
            }

            throw new HttpStatusException(StatusCodes.Status403Forbidden, "Only Project leaders or company owners can access this resource");
        }

        public async Task AssignUserAsync(ObjectId issueId, ObjectId userId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            if (await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, project, ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner))
            {
                if (!issue.AssignedUserIds.Contains(userId))
                    issue.AssignedUserIds.Add(userId);
                await _issueRepo.UpdateAsync(issue);
            }

            throw new HttpStatusException(StatusCodes.Status403Forbidden, "Only Project leaders or company owners can access this resource");
        }

        public async Task UnassignUserAsync(ObjectId issueId, ObjectId userId)
        {
            if (userId.Equals(_contextAccessor.HttpContext.User.GetUserId()))
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "You cannot unassign yourself from a project");
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            if (await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, project, ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner))
            {
                if (issue.AssignedUserIds.Remove(userId))
                    await _issueRepo.UpdateAsync(issue);
            }

            throw new HttpStatusException(StatusCodes.Status403Forbidden, "Only Project leaders or company owners can access this resource");
        }
    }
}