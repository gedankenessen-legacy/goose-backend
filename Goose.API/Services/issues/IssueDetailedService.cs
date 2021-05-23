using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Services.issues;
using Goose.API.Services.Issues;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Issues;
using Goose.API.Utils.Exceptions;
using Goose.API.Utils.Validators;
using Microsoft.AspNetCore.Http;
using Goose.Domain.Models.Identity;
using Microsoft.AspNetCore.Http.Features;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;

namespace Goose.API.Services.Issues
{
    public interface IIssueDetailedService
    {
        public Task<IList<IssueDTODetailed>> GetAllOfProject(ObjectId projectId, bool getAssignedUsers,
            bool getConversations, bool getTimeSheets, bool getParent, bool getChildren, bool getPredecessors, bool getSuccessors, bool getAll);

        public Task<IssueDTODetailed> Get(ObjectId projectId, ObjectId issueId, bool getAssignedUsers,
            bool getConversations, bool getTimeSheets, bool getParent, bool getChildren, bool getPredecessors, bool getSuccessors, bool getAll);
    }

    public class IssueDetailedService : IIssueDetailedService
    {
        private readonly IIssueConversationService _conversationService;
        private readonly IIssueTimeSheetService _timeSheetService;
        private readonly IIssueService _issueService;
        private readonly IIssueParentService _issueParentService;
        private readonly IIssueChildrenService _issueChildrenService;

        private readonly IIssueRepository _issueRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIssueRequestValidator _issueValidator;

        public IssueDetailedService(IIssueConversationService conversationService,
            IIssueTimeSheetService timeSheetService, IIssueService issueService, IUserRepository userRepository,
            IIssueRepository issueRepository, IIssueRequestValidator issueValidator, IIssueParentService issueParentService,
            IIssueChildrenService issueChildrenService)
        {
            _conversationService = conversationService;
            _timeSheetService = timeSheetService;
            _issueService = issueService;
            _userRepository = userRepository;
            _issueRepository = issueRepository;
            _issueValidator = issueValidator;
            _issueParentService = issueParentService;
            _issueChildrenService = issueChildrenService;
        }

        public async Task<IList<IssueDTODetailed>> GetAllOfProject(ObjectId projectId, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getChildren, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            if (!await _issueValidator.HasExistingProjectId(projectId))
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"Project with id [{projectId}] does not exist");

            var issues = await _issueRepository.GetAllOfProjectAsync(projectId);

            if (await _issueService.UserCanSeeInternTicket(projectId))
            {
                var tasks = issues.Select(it => CreateDtoFromIssue(it, getAssignedUsers, getConversations,
                    getTimeSheets, getParent, getChildren, getPredecessors, getSuccessors, getAll)).ToList();
                return (await Task.WhenAll(tasks)).ToList();
            }

            IList<Task<IssueDTODetailed>> issueTaskList = new List<Task<IssueDTODetailed>>();
            foreach (var issue in issues)
            {
                if (!issue.IssueDetail.Visibility)
                    continue;

                issueTaskList.Add(CreateDtoFromIssue(issue, getAssignedUsers, getConversations,
                    getTimeSheets, getParent, getChildren, getPredecessors, getSuccessors, getAll));
            }

            return (await Task.WhenAll(issueTaskList)).ToList();
        }

        public async Task<IssueDTODetailed> Get(ObjectId projectId, ObjectId issueId, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getChildren, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            if (!await _issueValidator.HasExistingProjectId(projectId))
                throw new HttpStatusException(StatusCodes.Status404NotFound, $"Project with id [{projectId}] does not exist");

            var issue = await _issueRepository.GetAsync(issueId);

            if (!issue.IssueDetail.Visibility && !await _issueService.UserCanSeeInternTicket(projectId))
                throw new HttpStatusException(StatusCodes.Status403Forbidden, $"Sie haben nicht die berechtigung dieses Ticket zu sehen");

            return await CreateDtoFromIssue(issue, getAssignedUsers, getConversations, getTimeSheets, getParent, getChildren, getPredecessors, getSuccessors,
                getAll);
        }

        private async Task<IssueDTODetailed> CreateDtoFromIssue(Issue issue, bool getAssignedUsers,
            bool getConversations,
            bool getTimeSheets, bool getParent, bool getChildren, bool getPredecessors, bool getSuccessors, bool getAll)
        {
            var issueDto = _issueService.Get(issue.Id);
            var assignedUsers = getAll || getAssignedUsers ? Task.WhenAll(issue.AssignedUserIds.Select(_userRepository.GetAsync)) : null;
            var conversations = getAll || getConversations ? _conversationService.GetConversationsFromIssueAsync(issue.Id.ToString()) : null;
            var timeSheets = getAll || getTimeSheets ? _timeSheetService.GetAllOfIssueAsync(issue.Id) : null;
            var parent = getAll || getParent ? _issueParentService.GetParent(issue.Id) : null;
            var children = getAll || getChildren ? _issueChildrenService.GetAll(issue.Id) : null;
            var predecessors = getAll || getPredecessors ? Task.WhenAll(issue.PredecessorIssueIds.Select(_issueService.Get)) : null;
            var successors = getAll || getSuccessors ? Task.WhenAll(issue.SuccessorIssueIds.Select(_issueService.Get)) : null;

            return new IssueDTODetailed((await issueDto).Id, (await issueDto).State, (await issueDto).Project, (await issueDto).Client,
                (await issueDto).Author, assignedUsers != null ? (await assignedUsers).Select(it => new UserDTO(it)).ToList() : null,
                conversations != null ? await conversations : null, timeSheets != null ? await timeSheets : null,
                issue.IssueDetail, parent != null ? await parent : null, children != null ? await children : null,
                predecessors != null ? await predecessors : null, successors != null ? await successors : null);
        }
    }
}