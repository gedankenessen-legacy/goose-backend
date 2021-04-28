using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Repositories;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.API.Utils;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssueTimeSheetService
    {
        public Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId);
        public Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheet);
        public Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto);
        public Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId);
    }

    public class IssueTimeSheetService : IIssueTimeSheetService

    {
        private readonly IIssueRepository _issueRepo;
        private readonly IUserRepository _userRepo;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IProjectRepository _projectRepository;
        private readonly ICompanyRepository _companyRepository;

        public IssueTimeSheetService(IIssueRepository issueRepo, IUserRepository userRepo, IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository, ICompanyRepository companyRepository)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
            _companyRepository = companyRepository;
        }

        public async Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var company = _companyRepository.GetAsync(project.CompanyId);

            #region UserIsEmployee

            if (await _authorizationService.HasAtLeastOneRequirement(user, project, ProjectRolesRequirement.EmployeeRequirement,
                    ProjectRolesRequirement.LeaderRequirement,
                    ProjectRolesRequirement.ReadonlyEmployeeRequirement) ||
                await _authorizationService.HasAtLeastOneRequirement(user, await company, CompanyRolesRequirement.CompanyOwner))
            {
                var timeSheets = (await _issueRepo.GetAsync(issueId)).TimeSheets;
                return (await Task.WhenAll(timeSheets.Select(MapToTimeSheetDTO))).ToList();
            }

            #endregion


            throw new HttpStatusException(StatusCodes.Status403Forbidden,
                $"User [{user.GetUserId()}] is not an project employee or company owner");
        }

        public async Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var company = _companyRepository.GetAsync(project.CompanyId);

            #region UserIsEmployee

            if (await _authorizationService.HasAtLeastOneRequirement(user!, project,
                    ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement,
                    ProjectRolesRequirement.ReadonlyEmployeeRequirement
                )
                || await _authorizationService.HasAtLeastOneRequirement(user!, await company,
                    CompanyRolesRequirement.CompanyOwner))
            {
                var timeSheet = (await _issueRepo.GetAsync(issueId)).TimeSheets.First(it => it.Id.Equals(timeSheetId));
                return await MapToTimeSheetDTO(timeSheet);
            }

            #endregion


            throw new HttpStatusException(StatusCodes.Status403Forbidden,
                $"User [{user.GetUserId()}] is not an project employee or company owner");
        }

        public async Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var company = _companyRepository.GetAsync(project.CompanyId);

            #region UserIsEmployee

            if (await _authorizationService.HasAtLeastOneRequirement(user!, project,
                    ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement)
                || await _authorizationService.HasAtLeastOneRequirement(user, await company, CompanyRolesRequirement.CompanyOwner))
            {
                timeSheetDto.Id = ObjectId.GenerateNewId();
                issue.TimeSheets.Add(timeSheetDto.ToTimeSheet());
                await _issueRepo.UpdateAsync(issue);
                return timeSheetDto;
            }

            #endregion


            throw new HttpStatusException(StatusCodes.Status403Forbidden,
                $"User [{user.GetUserId()}] does not have a role in this project");
        }

        public async Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var company = _companyRepository.GetAsync(project.CompanyId);

            #region UserIsEmployee

            if (await _authorizationService.HasAtLeastOneRequirement(user!, project,
                    ProjectRolesRequirement.EmployeeRequirement, ProjectRolesRequirement.LeaderRequirement)
                || await _authorizationService.HasAtLeastOneRequirement(user!, await company,
                    CompanyRolesRequirement.CompanyOwner))
            {
                issue.TimeSheets.Replace(it => it.Id == timeSheetDto.Id, timeSheetDto.ToTimeSheet());
                await _issueRepo.UpdateAsync(issue);
            }

            #endregion


            throw new HttpStatusException(StatusCodes.Status403Forbidden,
                $"User [{user.GetUserId()}] does not have a role in this project");
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var issue = await _issueRepo.GetAsync(issueId);
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var company = _companyRepository.GetAsync(project.CompanyId);

            #region UserIsEmployee

            if (await _authorizationService.HasAtLeastOneRequirement(user!, project,
                    ProjectRolesRequirement.LeaderRequirement)
                || await _authorizationService.HasAtLeastOneRequirement(user!, await company,
                    CompanyRolesRequirement.CompanyOwner))
            {
                var timeSheet = issue.TimeSheets.First(it => it.Id.Equals(timeSheetId));
                issue.TimeSheets.Remove(timeSheet);
                await _issueRepo.UpdateAsync(issue);
            }

            #endregion

            throw new HttpStatusException(StatusCodes.Status403Forbidden,
                $"User [{user.GetUserId()}] does not have a role in this project");
        }

        private async Task<IssueTimeSheetDTO> MapToTimeSheetDTO(TimeSheet timeSheet)
        {
            var user = await _userRepo.GetAsync(timeSheet.UserId);
            return new IssueTimeSheetDTO(timeSheet, user);
        }
    }
}