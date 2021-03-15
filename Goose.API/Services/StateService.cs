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
                _id = ObjectId.GenerateNewId(),
                Phase = requestedState.Phase,
                Name = requestedState.Name,
            };

            await _projectRepository.AddState(projectId, state);

            return new StateDTO(state);
        }

        public async Task<StateDTO> GetState(ObjectId projectId, ObjectId stateId)
        {
            var project = await _projectRepository.GetAsync(projectId);

            var matchedState = from state in project.States
                        where state._id == stateId
                        select new StateDTO(state);

            return matchedState.Single();
        }

        public async Task<IList<StateDTO>> GetStates(ObjectId projectId)
        {
            var project = await _projectRepository.GetAsync(projectId);
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
