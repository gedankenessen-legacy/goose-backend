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
    }
}
