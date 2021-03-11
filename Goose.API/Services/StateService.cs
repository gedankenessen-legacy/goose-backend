using Goose.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.Services
{
    public interface IStateService
    {
        Task<StateDTO> CreateState(StateDTO requestedState);
    }

    public class StateService : IStateService
    {
        public Task<StateDTO> CreateState(StateDTO requestedState)
        {
            throw new NotImplementedException();
        }
    }
}
