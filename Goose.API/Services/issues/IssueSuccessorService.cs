using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IAuthorizationService _authorizationService;
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public IssueSuccessorService(IIssueRepository issueRepo, IIssueService issueService, IAuthorizationService authorizationService, IProjectRepository projectRepository, IHttpContextAccessor contextAccessor)
        {
            _issueRepo = issueRepo;
            _issueService = issueService;
            _authorizationService = authorizationService;
            _projectRepository = projectRepository;
            _contextAccessor = contextAccessor;
        }

        public async Task<IList<IssueDTO>> GetAll(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, project,
                CompanyRolesRequirement.CompanyOwner, ProjectRolesRequirement.CustomerRequirement, ProjectRolesRequirement.EmployeeRequirement,
                ProjectRolesRequirement.ReadonlyEmployeeRequirement, ProjectRolesRequirement.LeaderRequirement))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"the user {_contextAccessor.HttpContext.User.GetUserId()} does not have a role in this project");

            return await Task.WhenAll(issue.SuccessorIssueIds.Select(it => _issueService.Get(it)));
        }
    }
}