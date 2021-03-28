using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Tickets;
using MongoDB.Bson;

namespace Goose.API.Services.Issues
{
    public interface IIssueTimeSheetService
    {
        public Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId);
        public Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId);
        public Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheet);
        public Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDTO);
        public Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId);
    }

    public class IssueTimeSheetService : IIssueTimeSheetService

    {
        private readonly IIssueRepository _issueRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public IssueTimeSheetService(IIssueRepository issueRepo, IMapper mapper, IUserRepository userRepo)
        {
            _issueRepo = issueRepo;
            _mapper = mapper;
            _userRepo = userRepo;
        }

        public async Task<IList<IssueTimeSheetDTO>> GetAllOfIssueAsync(ObjectId issueId)
        {
            var timesheets = (await _issueRepo.GetAsync(issueId)).TimeSheets;
            var dtos = await Task.WhenAll(timesheets.Select(MapToTimeSheetDTO));
            return dtos;
        }

        public async Task<IssueTimeSheetDTO> GetAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var timeSheet = (await _issueRepo.GetAsync(issueId)).TimeSheets.First(it => it.Id.Equals(timeSheetId));
            return await MapToTimeSheetDTO(timeSheet);
        }

        public async Task<IssueTimeSheetDTO> CreateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheet)
        {
            //TODO nicht atomar
            var issue = await _issueRepo.GetAsync(issueId);
            issue.TimeSheets.Add(_mapper.Map<TimeSheet>(timeSheet));
            await _issueRepo.UpdateAsync(issue);
            return timeSheet;
        }

        public async Task UpdateAsync(ObjectId issueId, IssueTimeSheetDTO timeSheetDTO)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var timeSheet = issue.TimeSheets.First(it => it.Id.Equals(timeSheetDTO.Id));
            SetTimeSheetFields(timeSheet, timeSheetDTO);
            await _issueRepo.UpdateAsync(issue);
        }

        public async Task DeleteAsync(ObjectId issueId, ObjectId timeSheetId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var timeSheet = issue.TimeSheets.First(it => it.Id.Equals(timeSheetId));
            issue.TimeSheets.Remove(timeSheet);
            await _issueRepo.UpdateAsync(issue);
        }


        private void SetTimeSheetFields(TimeSheet dest, IssueTimeSheetDTO source)
        {
            dest.Start = source.Start;
            dest.End = source.End;
        }

        private async Task<IssueTimeSheetDTO> MapToTimeSheetDTO(TimeSheet timeSheet)
        {
            var user = await _userRepo.GetAsync(timeSheet.UserId);
            var dto = _mapper.Map<IssueTimeSheetDTO>(timeSheet);
            dto.User = _mapper.Map<UserDTO>(user);
            return dto;
        }
    }
}