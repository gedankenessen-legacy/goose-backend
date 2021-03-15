using Goose.API.Repositories;
using Goose.Domain.DTOs;
using Goose.Domain.Models.projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IStateService
    {
        Task<StateDTO> CreateStateAsync(ObjectId projectId, StateDTO requestedState);
        Task UpdateState(ObjectId projectId, ObjectId stateId, StateDTO stateDTO);
        Task<IList<StateDTO>> GetStates(ObjectId projectId);
        Task<StateDTO> GetState(ObjectId projectId, ObjectId stateId);
    }

    public class StateService : IStateService
    {
        private readonly IProjectRepository _projectRepository;

        public StateService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<StateDTO> CreateStateAsync(ObjectId projectId, StateDTO requestedState)
        {
            var state = new State()
            {
                Id = ObjectId.GenerateNewId(),
                Phase = requestedState.Phase,
                Name = requestedState.Name,
            };

            await _projectRepository.AddState(projectId, state);

            return new StateDTO(state);
        }

        public async Task<StateDTO> GetState(ObjectId projectId, ObjectId stateId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                // TODO throw a HttpStatusException
                throw new Exception("Project not found");
            }

            if (project.States is null)
            {
                // TODO throw a HttpStatusException
                throw new Exception("State not found");
            }

            var matchedState = from state in project.States
                        where state.Id == stateId
                        select new StateDTO(state);

            return matchedState.Single();
        }

        public async Task<IList<StateDTO>> GetStates(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            if (project is null)
            {
                // TODO throw a HttpStatusException
                throw new Exception("Project not found");
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
            if (stateDTO.Id != stateId)
            {
                throw new Exception("Cannot Update: State ID does not match");
            }

            await _projectRepository.UpdateState(projectId, stateId, stateDTO.Name, stateDTO.Phase);
        }
    }
}
