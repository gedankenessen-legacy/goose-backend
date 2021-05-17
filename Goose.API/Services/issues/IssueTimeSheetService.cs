using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.API.Utils;
using Goose.API.Utils.Authentication;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using Microsoft.AspNetCore.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Authorization;
using Goose.API.Utils.Authentication;
using Goose.Domain.Models;
using Goose.API.Utils.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Goose.API.Services.Issues
{
    public interface IIssueTimeSheetService
    {
        public Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId);
        public Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheet);
        public Task UpdateAsync(ObjectId issueId, ObjectId id, IssueTimeSheetDTO timeSheetDto);
        public Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId);
    }

    public class IssueTimeSheetService : IIssueTimeSheetService

    {
        private readonly IIssueRepository _issueRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMessageService _messageService;
        private readonly IProjectRepository _projectRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public IssueTimeSheetService(IIssueRepository issueRepo, IUserRepository userRepo, IMessageService messageService, IProjectRepository projectRepository, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _messageService = messageService;
            _projectRepository = projectRepository;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public async Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var timeSheets = (await _issueRepo.GetAsync(issueId)).TimeSheets;
            return (await Task.WhenAll(timeSheets.Select(MapToTimeSheetDTO))).ToList();
        }

        public async Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var timeSheet = (await _issueRepo.GetAsync(issueId)).TimeSheets.First(it => it.Id.Equals(timeSheetId));
            return await MapToTimeSheetDTO(timeSheet);
        }

        public async Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            if ((await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, IssueOperationRequirments.CreateOwnTimeSheets)).Succeeded is false)
                throw new HttpStatusException(StatusCodes.Status403Forbidden, "You are not allowed to create a time sheet.");
            var timesheet = timeSheetDto.ToTimeSheet();

            timesheet.Id = ObjectId.GenerateNewId();
            timesheet.UserId = _httpContextAccessor.HttpContext.User.GetUserId();
            issue.TimeSheets.Add(timesheet);
            await _issueRepo.UpdateAsync(issue);
            await CreateTimeExccededMessage(issueId, timeSheetDto);
            return await GetAsync(issueId, timesheet.Id);
        }

        public async Task UpdateAsync(ObjectId issueId, ObjectId id, IssueTimeSheetDTO timeSheetDto)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            // updating own timesheets require other requirements.
            if (_httpContextAccessor.HttpContext.User.GetUserId().Equals(timeSheetDto.User.Id))
            {
                await CanUserUpdateOwnTimeSheetAsync(issue);
            }
            else
            {
                await CanUserUpdateTimeSheetAsync(issue);
                // Send Message to User
                await CreateTimeChangedMessage(issue, timeSheetDto);
            }
                

            issue.TimeSheets.Replace(it => it.Id == timeSheetDto.Id, timeSheetDto.ToTimeSheet());
            await _issueRepo.UpdateAsync(issue);
            await CreateTimeExccededMessage(issueId, timeSheetDto);
        }

        private async Task CanUserUpdateTimeSheetAsync(Issue issue)
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { IssueOperationRequirments.EditAllTimeSheets, "Your are not allowed to update a timesheet." }
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }

        private async Task CanUserUpdateOwnTimeSheetAsync(Issue issue)
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { IssueOperationRequirments.EditOwnTimeSheets, "Your are not allowed to update your timesheet." }
            };

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var timeSheet = issue.TimeSheets.First(it => it.Id.Equals(timeSheetId));
            issue.TimeSheets.Remove(timeSheet);
            await _issueRepo.UpdateAsync(issue);
        }

        private async Task<IssueTimeSheetDTO> MapToTimeSheetDTO(TimeSheet timeSheet)
        {
            var user = await _userRepo.GetAsync(timeSheet.UserId);
            return new IssueTimeSheetDTO(timeSheet, user);
        }

        private async Task CreateTimeExccededMessage(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            if (timeSheetDto.End == default(DateTime))
                return;

            var updatedIssue = await _issueRepo.GetAsync(issueId);
            var timesheets = updatedIssue.TimeSheets.Where(x => !x.End.Equals(default(DateTime)));

            TimeSpan? diffrence = new TimeSpan();

            foreach (var timesheet in timesheets)
                diffrence += timesheet.End - timesheet.Start;

            if (updatedIssue.IssueDetail.ExpectedTime is null || TimeSpan.FromSeconds((double)updatedIssue.IssueDetail.ExpectedTime * 3600) > diffrence)
                return;

            await _messageService.CreateMessageAsync(new Message()
            {
                CompanyId = (await _projectRepository.GetAsync(updatedIssue.ProjectId)).CompanyId,
                ProjectId = updatedIssue.ProjectId,
                IssueId = updatedIssue.Id,
                ReceiverUserId = updatedIssue.ClientId,
                Type = MessageType.TimeExceeded,
                Consented = false,
            });
        }

        private async Task CreateTimeChangedMessage(Issue issue, IssueTimeSheetDTO timeSheetDto)
        {
            await _messageService.CreateMessageAsync(new Message()
            {
                CompanyId = (await _projectRepository.GetAsync(issue.ProjectId)).CompanyId,
                ProjectId = issue.ProjectId,
                IssueId = issue.Id,
                ReceiverUserId = timeSheetDto.User.Id,
                Type = MessageType.RecordedTimeChanged,
                Consented = false,
            });
        }
    }
}