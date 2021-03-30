using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.issues;
using Goose.Domain.DTOs.tickets;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.tickets;
using Microsoft.AspNetCore.Http.Features;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueDetailedService
    {
        public Task<IList<IssueDTODetailed>> GetAllOfProject(ObjectId projectId, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getPredecessors, bool getSuccessors, bool getAll);

        public Task<IssueDTODetailed> Get(ObjectId issueId, bool getAssignedUsers, bool getConversations,
            bool getTimeSheets, bool getParent, bool getPredecessors, bool getSuccessors, bool getAll);
    }

    public class IssueDetailedService: IIssueDetailedService
    {
        private readonly IIssueConversationService _conversationService;
        private readonly IIssueTimeSheetService _timeSheetService;
        private readonly IIssueService _issueService;

        private readonly IIssueRepository _issueRepository;
        private readonly IUserRepository _userRepository;

        public IssueDetailedService(IIssueConversationService conversationService,
            IIssueTimeSheetService timeSheetService, IIssueService issueService, IUserRepository userRepository,
            IIssueRepository issueRepository)
        {
            _conversationService = conversationService;
            _timeSheetService = timeSheetService;
            _issueService = issueService;
            _userRepository = userRepository;
            _issueRepository = issueRepository;
        }

        public async Task<IList<IssueDTODetailed>> GetAllOfProject(ObjectId projectId, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            var issues = await _issueRepository.GetAllOfProjectAsync(projectId);
            var first = issues.ElementAtOrDefault(0);
            var dto = CreateDtoFromIssue(first, getAssignedUsers, getConversations,
                getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
            var tasks = issues.Select(it => CreateDtoFromIssue(it, getAssignedUsers, getConversations,
                getTimeSheets, getParent, getPredecessors, getSuccessors, getAll));
            return await Task.WhenAll(tasks);
        }

        public async Task<IssueDTODetailed> Get(ObjectId issueId, bool getAssignedUsers, bool getConversations,
            bool getTimeSheets, bool getParent, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            var issue = await _issueRepository.GetAsync(issueId);
            return await CreateDtoFromIssue(issue, getAssignedUsers, getConversations,
                getTimeSheets, getParent, getPredecessors, getSuccessors, getAll);
        }

        private async Task<IssueDTODetailed> CreateDtoFromIssue(Issue issue, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            var issueDto = _issueService.Get(issue.Id);
            var assignedUsers = getAll || getAssignedUsers
                ? Task.WhenAll(issue.AssignedUserIds.Select(_userRepository.GetAsync))
                : NullTask<User[]>();
            var conversations = getAll || getConversations
                ? _conversationService.GetConversationsFromIssueAsync(issue.Id.ToString())
                : NullTask<IList<IssueConversationDTO>>();
            var timeSheets = getAll || getTimeSheets
                ? _timeSheetService.GetAllOfIssueAsync(issue.Id)
                : NullTask<IList<IssueTimeSheetDTO>>();
            var parent = getAll || getParent
                ? _issueService.GetParent(issue.Id)
                : NullTask<IssueDTO?>();
            var predecessors = getAll || getPredecessors
                ? Task.WhenAll(issue.PredecessorIssueIds.Select(_issueService.Get))
                : NullTask<IssueDTO[]>();
            var successors = getAll || getSuccessors
                ? Task.WhenAll(issue.SuccessorIssueIds.Select(_issueService.Get))
                : NullTask<IssueDTO[]>();

            return new IssueDTODetailed((await issueDto).State, (await issueDto).Project, (await issueDto).Client,
                (await issueDto).Author, (await assignedUsers).Select(it => new UserDTO(it)).ToList(),
                await conversations, await timeSheets, issue.IssueDetail, await parent, await predecessors,
                await successors);
        }

        private async Task<T> NullTask<T>()
        {
            return default;
        }
    }
}