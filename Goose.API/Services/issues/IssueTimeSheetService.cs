using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils;
using Goose.Domain.DTOs.issues;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.API.Services.issues
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

        public IssueTimeSheetService(IIssueRepository issueRepo, IUserRepository userRepo)
        {
            _issueRepo = issueRepo;
            _userRepo = userRepo;
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
            return timeSheetDto;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDto)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            issue.TimeSheets.Replace(it => it.Id == timeSheetDto.Id, timeSheetDto.ToTimeSheet());
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