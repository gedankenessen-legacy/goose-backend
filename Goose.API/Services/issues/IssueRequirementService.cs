using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
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
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public IssueRequirementService(IIssueRepository issueRepository, IHttpContextAccessor contextAccessor, IAuthorizationService authorizationService,
            IProjectRepository projectRepository)
        {
            _issueRepository = issueRepository;
            _contextAccessor = contextAccessor;
            _authorizationService = authorizationService;
            _projectRepository = projectRepository;
        }

        public async Task<IList<IssueRequirement>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var issue = await _issueRepository.GetAsync(issueId) ??
                        throw new HttpStatusException(StatusCodes.Status400BadRequest, $"the issue {issueId} does not exist");
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, await _projectRepository.GetAsync(issue.ProjectId),
                ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement, ProjectRolesRequirement.ReadonlyEmployeeRequirement,
                CompanyRolesRequirement.CompanyOwner, ProjectRolesRequirement.CustomerRequirement))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"The user {_contextAccessor.HttpContext.User.GetUserId()} does not have a role in this project");

            return issue.IssueDetail.Requirements;
        }

        public async Task<IssueRequirement> GetAsync(ObjectId issueId, ObjectId requirementId)
        {
            var issue = await _issueRepository.GetAsync(issueId) ??
                        throw new HttpStatusException(StatusCodes.Status400BadRequest, $"the issue {issueId} does not exist");
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, await _projectRepository.GetAsync(issue.ProjectId),
                ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement, ProjectRolesRequirement.ReadonlyEmployeeRequirement,
                CompanyRolesRequirement.CompanyOwner, ProjectRolesRequirement.CustomerRequirement))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"The user {_contextAccessor.HttpContext.User.GetUserId()} does not have a role in this project");

            return issue.IssueDetail.Requirements.First(it => it.Id.Equals(requirementId));
        }

        public async Task<IssueRequirement> CreateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            var issue = await _issueRepository.GetAsync(issueId) ??
                        throw new HttpStatusException(StatusCodes.Status400BadRequest, $"the issue {issueId} does not exist");
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, await _projectRepository.GetAsync(issue.ProjectId),
                ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement,
                CompanyRolesRequirement.CompanyOwner))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"The user {_contextAccessor.HttpContext.User.GetUserId()} is not an employee or company of this project");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400,
                    "Eine Anforderung für dieses Ticket konnte nicht erstellt werden, da schon eine Zusammenfassung erstellt wurde");

            if (string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine Valide Anforderung an");

            if (issue.IssueDetail.Requirements is null)
                issue.IssueDetail.Requirements = new List<IssueRequirement>();

            requirement.Id = ObjectId.GenerateNewId();
            issue.IssueDetail.Requirements.Add(requirement);
            await _issueRepository.UpdateAsync(issue);

            return requirement;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueRequirement requirement)
        {
            var issue = await _issueRepository.GetAsync(issueId) ??
                        throw new HttpStatusException(StatusCodes.Status400BadRequest, $"the issue {issueId} does not exist");
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, await _projectRepository.GetAsync(issue.ProjectId),
                ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement, CompanyRolesRequirement.CompanyOwner))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"The user {_contextAccessor.HttpContext.User.GetUserId()} is not an employee or company of this project");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400,
                    "Die Anforderung für dieses Ticket konnte nicht aktualliesiert werden, da schon eine Zusammenfassung erstellt wurde");

            if (string.IsNullOrWhiteSpace(requirement.Requirement))
                throw new HttpStatusException(400, "Bitte geben Sie eine Valide Anforderung an");

            var req = issue.IssueDetail.Requirements.First(it => it.Id == requirement.Id);
            SetRequirementFields(req, requirement);
            await _issueRepository.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId requirementId)
        {
            var issue = await _issueRepository.GetAsync(issueId) ??
                        throw new HttpStatusException(StatusCodes.Status400BadRequest, $"the issue {issueId} does not exist");
            if (!await _authorizationService.HasAtLeastOneRequirement(_contextAccessor.HttpContext.User, await _projectRepository.GetAsync(issue.ProjectId),
                ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement, CompanyRolesRequirement.CompanyOwner))
                throw new HttpStatusException(StatusCodes.Status403Forbidden,
                    $"The user {_contextAccessor.HttpContext.User.GetUserId()} is not an employee or company of this project");

            if (issue.IssueDetail.RequirementsSummaryCreated)
                throw new HttpStatusException(400,
                    "Die Anforderung für dieses Ticket konnte nicht entfernt werden, da schon eine Zusammenfassung erstellt wurde");

            var req = issue.IssueDetail.Requirements.First(it => it.Id.Equals(requirementId));
            issue.IssueDetail.Requirements.Remove(req);
            await _issueRepository.UpdateAsync(issue);
        }


        private void SetRequirementFields(IssueRequirement dest, IssueRequirement source)
        {
            dest.Requirement = source.Requirement;
        }
    }
}