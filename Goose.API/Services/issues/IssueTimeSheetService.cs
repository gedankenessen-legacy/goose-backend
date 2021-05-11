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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IssueTimeSheetService(IIssueRepository issueRepo, IUserRepository userRepo, IHttpContextAccessor httpContextAccessor)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
            _httpContextAccessor = httpContextAccessor;
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
            var timesheet = timeSheetDto.ToTimeSheet();

            timesheet.Id = ObjectId.GenerateNewId();
            timesheet.UserId = _httpContextAccessor.HttpContext.User.GetUserId();
            issue.TimeSheets.Add(timesheet);
            await _issueRepo.UpdateAsync(issue);
            return await GetAsync(issueId, timesheet.Id);
        }

        public async Task UpdateAsync(ObjectId issueId, ObjectId id, IssueTimeSheetDTO timeSheetDto)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var timeSheet = issue.TimeSheets.FirstOrDefault(it => it.Id.Equals(id));
            if (timeSheet == null) throw new HttpStatusException(StatusCodes.Status400BadRequest, $"There is no timesheet with the id [{id}]");

            timeSheet.Start = timeSheetDto.Start;
            timeSheet.End = timeSheetDto.End;
            await _issueRepo.UpdateAsync(issue);
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
    }
}