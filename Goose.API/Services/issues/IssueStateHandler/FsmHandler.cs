using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Services.issues.IssueStateHandler;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Issues;

namespace Goose.API.Services.issues.IssueStateHandler
{
    public class FsmHandler
    {
        private readonly IIssueRepository _issueRepository;
        private List<IFsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>>> _events = new();

        //maps which state a usergenerated state in a specific phase belongs to
        private Dictionary<string, string> _userGeneratedStatesInPhase = new();

        public FsmHandler(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public void AddUserGeneratedStateCase(string phase, string correspondingState)
        {
            _userGeneratedStatesInPhase.Add(phase, correspondingState);
        }

        public void AddEvent(IFsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> e)
        {
            _events.Add(e);
        }

        public async Task<StateDTO> HandleStateUpdate(Issue issue, StateDTO oldState, StateDTO newState)
        {
            if (issue == null || oldState == null || newState == null)
                throw new HttpStatusException(400, "Invalid request, Issue or State does not exist");

            var copyOldState = new StateDTO
            {
                Phase = oldState.Phase,
                Name = oldState.UserGenerated ? _userGeneratedStatesInPhase[oldState.Phase] : oldState.Name
            };
            var copyNewState = new StateDTO
            {
                Phase = newState.Phase,
                Name = newState.UserGenerated ? _userGeneratedStatesInPhase[newState.Phase] : newState.Name
            };

            if (copyOldState.Name == copyNewState.Name)
                return await SetState(issue, newState);

            var e = Find(copyOldState, copyNewState);
            if (e == null) throw new HttpStatusException(400, $"cannot update an issue state from {oldState.Name} to {newState.Name}");
            return await CallAction(e.Func, issue, oldState, newState);
        }

        private async Task<StateDTO> SetState(Issue issue, StateDTO state)
        {
            issue.StateId = state.Id;
            await _issueRepository.UpdateAsync(issue);
            return state;
        }

        public async Task<StateDTO> CallAction(Func<Issue, StateDTO, StateDTO, Task<StateDTO>> action, Issue issue, StateDTO oldState, StateDTO newState)
        {
            return await action(issue, oldState, newState);
        }

        public IFsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>>? Find(StateDTO oldState, StateDTO newState)
        {
            return _events.FirstOrDefault(it => it.IsValidEvent(oldState, newState));
        }
    }
}