using Goose.API.Repositories;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Goose.API.Authorization.Requirements;
using Microsoft.AspNetCore.Http;
using Goose.API.Authorization;

namespace Goose.API.Services
{
    public interface IStateService
    {
        Task<StateDTO> CreateStateAsync(ObjectId projectId, StateDTO requestedState);
        Task UpdateState(ObjectId projectId, ObjectId stateId, StateDTO stateDTO);
        Task<IList<StateDTO>> GetStates(ObjectId projectId);
        Task<StateDTO> GetState(ObjectId projectId, ObjectId stateId);
        Task DeleteState(ObjectId projectId, ObjectId stateId);
    }

    public class StateService : IStateService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRepository _issueRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StateService(IProjectRepository projectRepository, IIssueRepository issueRepository, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StateDTO> CreateStateAsync(ObjectId projectId, StateDTO requestedState)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                throw new HttpStatusException(404, "Project not found");
            }

            await AuthorizeEmployeeAndLeaderRolesAsync(project);

            var state = new State()
            {
                Id = ObjectId.GenerateNewId(),
                Phase = requestedState.Phase,
                Name = requestedState.Name,
                UserGenerated = true,
            };

            await _projectRepository.AddState(projectId, state);

            return new StateDTO(state);
        }

        public async Task<StateDTO> GetState(ObjectId projectId, ObjectId stateId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                throw new HttpStatusException(404, "Project not found");
            }

            if (project.States is null)
            {
                throw new HttpStatusException(404, "State not found");
            }

            var matchedState = from state in project.States
                        where state.Id == stateId
                        select new StateDTO(state);

            return matchedState.SingleOrDefault();
        }

        public async Task<IList<StateDTO>> GetStates(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                throw new HttpStatusException(404, "Project not found");
            }

            if (project.States is null)
            {
                return new List<StateDTO>();
            }

            var states = from state in project.States
                         select new StateDTO(state);

            return states.ToList();
        }

        public async Task UpdateState(ObjectId projectId, ObjectId stateId, StateDTO stateDTO)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                throw new HttpStatusException(404, "Project not found");
            }

            await AuthorizeEmployeeAndLeaderRolesAsync(project);

            if (stateDTO.Id != stateId)
            {
                throw new HttpStatusException(400, "Cannot Update: State ID does not match");
            }

            await _projectRepository.UpdateState(projectId, stateId, stateDTO.Name, stateDTO.Phase);
        }

        public async Task DeleteState(ObjectId projectId, ObjectId stateId)
        {
            var issuesInState = await _issueRepository.FilterByAsync(x => x.ProjectId == projectId && x.StateId == stateId);

            if (issuesInState.Any())
            {
                // Es gibt Tickets in diesem Status, der Status darf nicht gelöscht werden
                throw new HttpStatusException(403, "Cannot delete because there are issues in this state");
            }

            var project = await _projectRepository.GetAsync(projectId);

            if (project == null)
            {
                throw new HttpStatusException(404, "Invalid projectId");
            }

            await AuthorizeEmployeeAndLeaderRolesAsync(project);

            var stateDeleted = project.States.Remove(state => {
                if (state.Id != stateId)
                {
                    return false;
                }

                // Wir haben den richtigen Status gefunden, aber darf er gelösch werden
                if (!state.UserGenerated)
                {
                    throw new HttpStatusException(403, "Cannot delete default State");
                }

                // Ja, er kann gelöscht werden
                return true;
            });

            if (stateDeleted)
            {
                await _projectRepository.UpdateAsync(project);
            }
        }

        private async Task<bool> AuthorizeEmployeeAndLeaderRolesAsync(Project project)
        {
            // Dict with the requirement as key und the error message as value.
            Dictionary<IAuthorizationRequirement, string> requirementsWithErrors = new()
            {
                { ProjectRolesRequirement.EmployeeRequirement, "You missing the employee role in this project, in order to create a state." },
                { ProjectRolesRequirement.LeaderRequirement, "You missing the leader role in this project, in order to create a state." },
            };

            // validate requirements with the appropriate handlers.
            (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, project, requirementsWithErrors.Keys)).ThrowErrorIfAllFailed(requirementsWithErrors);

            return true;
        }
    }
}
