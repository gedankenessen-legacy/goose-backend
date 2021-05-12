using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.API.Utils;
using MongoDB.Bson;
using System;
using Goose.Domain.Models;

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
        private readonly IMessageService _messageService;
        private readonly IProjectRepository _projectRepository;

        public IssueTimeSheetService(IIssueRepository issueRepo, IUserRepository userRepo, IMessageService messageService, IProjectRepository projectRepository)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _messageService = messageService;
            _projectRepository = projectRepository;
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

            timeSheetDto.Id = ObjectId.GenerateNewId();
            issue.TimeSheets.Add(timeSheetDto.ToTimeSheet());
            await _issueRepo.UpdateAsync(issue);
            await CreateTimeExccededMessage(issueId, timeSheetDto);
            return timeSheetDto;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            issue.TimeSheets.Replace(it => it.Id == timeSheetDto.Id, timeSheetDto.ToTimeSheet());
            await _issueRepo.UpdateAsync(issue);
            await CreateTimeExccededMessage(issueId, timeSheetDto);
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
    }
}