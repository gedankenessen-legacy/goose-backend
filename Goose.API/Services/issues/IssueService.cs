using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.API.Utils.Validators;
using Goose.Domain.DTOs;
using Goose.Domain.DTOs.Issues;
using Goose.Domain.Models.Projects;
using Goose.Domain.Models.Issues;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Goose.API.Utils.Authentication;
using Microsoft.AspNetCore.Authorization;
using Goose.API.Authorization.Requirements;
using Goose.API.Authorization;
using Goose.Domain.Models;
using Goose.API.EventHandler;
using System;

namespace Goose.API.Services.Issues
{
    public interface IIssueService
    {
        Task<IList<IssueDTO>> GetAll();
        public Task<IssueDTO> Get(ObjectId id);
        Task<IList<IssueDTO>> GetAllOfProject(ObjectId projectId);
        public Task<IssueDTO> GetOfProject(ObjectId projectId, ObjectId id);
        public Task<IssueDTO> Create(IssueDTO issueDto);
        public Task<IssueDTO> Update(IssueDTO issueDto, ObjectId id);
        public Task<bool> Delete(ObjectId id);
        public Task<IssueDTO?> GetParent(ObjectId issueId);
        public Task SetParent(ObjectId issueId, ObjectId parentId);
        public Task RemoveParent(ObjectId issueId);

        public Task AssertNotArchived(Issue issue);
        Task<bool> UserCanSeeInternTicket(ObjectId projectId);
    }

    public class IssueService : AuthorizableService, IIssueService
    {
        private readonly IStateService _stateService;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        private readonly IIssueRepository _issueRepo;
        private readonly IIssueRequestValidator _issueValidator;

        private readonly IMessageService _messageService;

        public IssueService(
            IIssueRepository issueRepo,
            IStateService stateService,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IIssueRequestValidator issueValidator,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IMessageService messageService) : base (httpContextAccessor, authorizationService)
        {
            _issueRepo = issueRepo;
            _stateService = stateService;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _issueValidator = issueValidator;
            _messageService = messageService;
        }

        public async Task<IList<IssueDTO>> GetAll()
        {
            return (await Task.WhenAll((await _issueRepo.GetAsync()).Select(CreateDtoFromIssue))).ToList();
        }

        public async Task<IssueDTO> Get(ObjectId id)
        {
            return await CreateDtoFromIssue(await _issueRepo.GetAsync(id));
        }

        public async Task<IList<IssueDTO>> GetAllOfProject(ObjectId projectId)
        {
            return (await Task.WhenAll((await _issueRepo.GetAllOfProjectAsync(projectId)).Select(CreateDtoFromIssue)))
                .ToList();
        }

        public async Task<IssueDTO> GetOfProject(ObjectId projectId, ObjectId id)
        {
            return await CreateDtoFromIssue(await _issueRepo.GetOfProjectAsync(projectId, id));
        }

        public async Task<IssueDTO> Create(IssueDTO issueDto)
        {
            if (!await _issueValidator.HasExistingProjectId(issueDto.Project.Id))
                throw new HttpStatusException(StatusCodes.Status400BadRequest,
                    $"Cannot create an Issue. Project with id [{issueDto.Project.Id}] does not exist");

            var issue = await CreateValidIssue(issueDto);

            await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.Create);

            await _issueRepo.CreateAsync(issue);

            if(issue.IssueDetail.EndDate is not null && issue.IssueDetail.EndDate != default(DateTime))
                await Scheduler.AddEvent(new IssueDeadlineEvent(await _projectRepository.GetAsync(issue.ProjectId), issue, _messageService, _issueRepo));

            return await Get(issue.Id);
        }

        private async Task<Issue> CreateValidIssue(IssueDTO dto)
        {
            if (dto.IssueDetail.RequirementsNeeded) dto.State = await GetState(dto.Project.Id, State.CheckingState);
            else dto.State = await GetState(dto.Project.Id, State.ProcessingState);
            var issue = new Issue
            {
                Id = dto.Id,
                StateId = dto.State!.Id,
                ProjectId = dto.Project.Id,
                ClientId = dto.Client.Id,
                AuthorId = dto.Author.Id,
                IssueDetail = CreateValidIssueDetail(dto.IssueDetail),
                ConversationItems = new List<IssueConversation>(),
                TimeSheets = new List<TimeSheet>(),
                AssignedUserIds = new List<ObjectId>(),
                ParentIssueId = null,
                PredecessorIssueIds = new List<ObjectId>(),
                SuccessorIssueIds = new List<ObjectId>()
            };

            return issue;
        }

        private IssueDetail CreateValidIssueDetail(IssueDetail detail)
        {
            //TODO more validation
            detail.Requirements = new List<IssueRequirement>();
            detail.RelevantDocuments = new List<string>();
            detail.RequirementsSummaryCreated = false;
            detail.RequirementsAccepted = false;
            return detail;
        }

        public async Task<IssueDTO> Update(IssueDTO issueDto, ObjectId id)
        {
            var issue = await _issueRepo.GetAsync(id);

            if(issue is null)
                throw new HttpStatusException(StatusCodes.Status400BadRequest, $"Das angeforderte Ticket mit der id {id} konnte nicht gefunden werden");

            await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.Edit);

            var issueToUpdate = await GetUpdatedIssue(issue, issueDto);

            await _issueRepo.UpdateAsync(issueToUpdate);
            return await Get(id);
        }

        private async Task<Issue> GetUpdatedIssue(Issue old, IssueDTO updated)
        {
            if (updated.State != null)
            {
                var oldStateId = old.StateId;
                var newStateId = updated.State.Id;

                if (oldStateId != newStateId)
                {
                    var oldState = await _stateService.GetState(old.ProjectId, oldStateId);
                    var newState = await _stateService.GetState(old.ProjectId, newStateId);

                    // if changing the state to cancelled, we need to validate the user requirments.
                    if (newState.Name.Equals(State.CancelledState))
                    {
                        await UserCanDiscardIssue(old);
                        await CreateCanceledMessage(old);
                    }
                    else
                        await AuthenticateRequirmentAsync(old, IssueOperationRequirments.EditState);

                    // State wird aktualisiert
                    old.StateId = newStateId;

                    old.ConversationItems.Add(new IssueConversation()
                    {
                        Id = ObjectId.GenerateNewId(),
                        CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                        Type = IssueConversation.StateChangeType,
                        Data = $"Status von {oldState.Name} zu {newState.Name} geändert.",
                        StateChange = new StateChange()
                        {
                            Before = oldState.Name,
                            After = newState.Name,
                        }
                    });
                }
            }

            old.IssueDetail = await GetUpdatedIssueDetail(old, updated.IssueDetail);
            return old;
        }

        private async Task CreateCanceledMessage(Issue issue)
        {
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            await CreateCanceledMessage(project.CompanyId, project.Id, issue.Id, issue.AuthorId);
            await CreateCanceledMessage(project.CompanyId, project.Id, issue.Id, issue.ClientId);
            await Task.WhenAll(issue.AssignedUserIds.Select(x => CreateCanceledMessage(project.CompanyId, project.Id, issue.Id, x)));
        }

        private async Task CreateCanceledMessage(ObjectId companyId, ObjectId projectId, ObjectId issueId, ObjectId userId)
        {
            await _messageService.CreateMessageAsync(new Message()
            {
                CompanyId = companyId,
                ProjectId = projectId,
                IssueId = issueId,
                ReceiverUserId = userId,
                Type = MessageType.IssueCancelled,
                Consented = false
            });
        }

        private async Task<IssueDetail> GetUpdatedIssueDetail(Issue old, IssueDetail updated)
        {
            var details = old.IssueDetail;
            details.Name = updated.Name;

            details.Priority = updated.Priority;
            details.Description = updated.Description;
            details.Progress = updated.Progress;
            details.ExpectedTime = updated.ExpectedTime;
            details.RelevantDocuments = updated.RelevantDocuments ?? details.RelevantDocuments;
            //nur in vorbereitungsphase
            var state = await _stateService.GetState(old.ProjectId, old.StateId);
            if (state.Phase.Equals(State.NegotiationPhase))
            {
                details.StartDate = updated.StartDate;
                details.EndDate = updated.EndDate;

                if (details.EndDate is not null && details.EndDate != default(DateTime))
                    await Scheduler.AddEvent(new IssueDeadlineEvent(await _projectRepository.GetAsync(old.ProjectId), old, _messageService, _issueRepo));
                else
                    await IssueDeadlineEvent.CancelDeadLine(old.Id);
            }

            return old.IssueDetail;
        }

        public async Task<bool> Delete(ObjectId id)
        {
            return (await _issueRepo.DeleteAsync(id)).DeletedCount > 0;
        }

        public async Task<IssueDTO?> GetParent(ObjectId issueId)
        {
            var parentId = (await _issueRepo.GetAsync(issueId)).ParentIssueId;
            if (parentId == null) return null;
            return await Get(issueId);
        }

        public async Task SetParent(ObjectId issueId, ObjectId parentId)
        {
            var issue = await _issueRepo.GetAsync(issueId);

            await AuthenticateRequirmentAsync(issue, IssueOperationRequirments.AddSubIssue);

            var parent = await _issueRepo.GetAsync(parentId);

            if (issue.ProjectId != parent.ProjectId)
            {
                throw new HttpStatusException(StatusCodes.Status400BadRequest, "Issues müssen im selben Projekt sein");
            }

            issue.ParentIssueId = parentId;
            await _issueRepo.UpdateAsync(issue);

            // ConversationItem im Oberticket hinzufügen
            parent.ConversationItems.Add(new IssueConversation()
            {
                Id = ObjectId.GenerateNewId(),
                CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                Type = IssueConversation.ChildIssueAddedType,
                Data = null,
                OtherTicketId = issueId,
            });
            await _issueRepo.UpdateAsync(parent);
        }

        public async Task RemoveParent(ObjectId issueId)
        {
            var issue = await _issueRepo.GetAsync(issueId);
            var mightBeParentId = issue.ParentIssueId;
            issue.ParentIssueId = null;
            await _issueRepo.UpdateAsync(issue);

            if (mightBeParentId is ObjectId parentId)
            {
                // ConversationItem im Oberticket hinzufügen
                var parent = await _issueRepo.GetAsync(parentId);
                parent.ConversationItems.Add(new IssueConversation()
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatorUserId = _httpContextAccessor.HttpContext.User.GetUserId(),
                    Type = IssueConversation.ChildIssueRemovedType,
                    Data = null,
                    OtherTicketId = issueId,
                });
                await _issueRepo.UpdateAsync(parent);
            }
        }

        private async Task<StateDTO> GetState(ObjectId projectId, string stateName)
        {
            var states = await _stateService.GetStates(projectId);
            var state = states.FirstOrDefault(it => it.Name.Equals(stateName));
            if (state == null)
                throw new HttpStatusException(StatusCodes.Status500InternalServerError, $"Project does not have a state with name [{stateName}]");
            return state;
        }

        private async Task<IssueDTO> CreateDtoFromIssue(Issue issue)
        {
            var state = _stateService.GetState(issue.ProjectId, issue.StateId);
            var project = _projectRepository.GetAsync(issue.ProjectId);
            var client = _userRepository.GetAsync(issue.ClientId);
            var author = _userRepository.GetAsync(issue.AuthorId);

            //TODO temporarily allowing state/project/client/author to be null
            return new IssueDTO(issue, await state, await project != null ? new ProjectDTO(await project) : null,
                await client != null ? new UserDTO(await client) : null,
                await author != null ? new UserDTO(await author) : null);
        }

        /// <summary>
        /// This method checks that the issue is not archived. If it is, it throws a
        /// HttpStatusException with 403 - Forbidden
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task AssertNotArchived(Issue issue)
        {
            var project = await _projectRepository.GetAsync(issue.ProjectId);
            var archivedState = project.States.Single(s => s.UserGenerated == false && s.Name == State.ArchivedState);

            if (issue.StateId == archivedState.Id)
            {
                throw new HttpStatusException(403, "Issue is archived.");
            }
        }

        public async Task<bool> UserCanSeeInternTicket(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
            IList<IAuthorizationRequirement> requirements = new List<IAuthorizationRequirement>()
            {
                ProjectRolesRequirement.EmployeeRequirement,
                ProjectRolesRequirement.LeaderRequirement,
                ProjectRolesRequirement.ReadonlyEmployeeRequirement,
                CompanyRolesRequirement.CompanyOwner
            };
            

            // validate requirements with the appropriate handlers.
            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, requirements);

            return authorizationResult.Failure.FailedRequirements.Count() < requirements.Count;
        }

        private async Task UserCanDiscardIssue(Issue issue)
        {
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { IssueOperationRequirments.DiscardIssue, "Your are not allowed to discard the issue." }
            };

            // add additional req. for internal issues.
            if (issue.IssueDetail.Visibility is false)
                requirementsWithErrors.Add(IssueOperationRequirments.EditStateOfInternal, "Your are not allowed to edit the state of an internal issue.");

            var authorizationResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, issue, requirementsWithErrors.Keys);
            authorizationResult.ThrowErrorForFailedRequirements(requirementsWithErrors);
        }
    }
}