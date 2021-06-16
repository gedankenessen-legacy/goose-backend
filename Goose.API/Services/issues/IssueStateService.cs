using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Services.issues.IssueStateHandler;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;

namespace Goose.API.Services.issues
{
    public interface IIssueStateService
    {
        public Task<StateDTO> UpdateState(Issue issue, StateDTO newState);
    }

    public class IssueStateService : IIssueStateService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IStateService _stateService;
        private readonly IIssueHelper _issueHelper;

        private readonly FsmHandler _updateStateHandler;

        public IssueStateService(IIssueRepository issueRepository, IStateService stateService, IIssueHelper _issueHelper)
        {
            _issueRepository = issueRepository;
            _stateService = stateService;
            this._issueHelper = _issueHelper;
            _updateStateHandler = new FsmHandler(issueRepository);

            RegisterFsmEvents();
            RegisterPossibleUsergeneratedStates();
        }

        public void RegisterFsmEvents()
        {
            Func<Issue, StateDTO, StateDTO, Task<StateDTO>> moveToProcessingPhase = async (Issue issue, StateDTO oldState, StateDTO newState) =>
            {
                //Das Issue wird in Waiting gesetzt wenn das Startdatum noch nicht erreicht ist
                if (issue.IssueDetail.StartDate > DateTime.Now)
                {
                    var waitingState = await GetStateByName(issue, State.WaitingState);
                    return await SetState(issue, waitingState);
                }

                Task<Issue> parent = null;
                if (issue.ParentIssueId is { } parentId) parent = _issueRepository.GetAsync(parentId);
                var predecessors = Task.WhenAll(issue.PredecessorIssueIds.Select(_issueRepository.GetAsync));


                Task<StateDTO> parentState = null;
                if (parent is { } parentNotNull) parentState = _stateService.GetState((await parentNotNull).ProjectId, (await parentNotNull).StateId);
                var predecessorStates = await Task.WhenAll((await predecessors).Select(it => _stateService.GetState(it.ProjectId, it.StateId)));

                /*
                 * Issue wird blockiert wenn:
                 * 1) Das Oberticket nicht in der Bearbeitungsphase ist
                 * 2) Nicht alle Vorgänger abgeschlossen sind
                 * 
                 */
                
                //hier muss aufgepasst werden !! 
                //parentSate ist ein Task d.h der Task an sich muss nicht null sein, wenn der Task dann 
                //aber awaited wird, kann es passieren, dass das ergebniss null ist
                if (parentState != null && (await parentState).Phase == State.NegotiationPhase ||
                    predecessorStates.HasWhere(it => it.Phase != State.ConclusionPhase))
                {
                    var blockedState = await GetStateByName(issue, State.BlockedState);
                    return await SetState(issue, blockedState);
                }

                return await SetState(issue, newState);
            };

            _updateStateHandler.AddEvent(
                new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //CheckingState -> NegotiationState
                {
                    OldState = State.CheckingState,
                    NewState = State.NegotiationState,
                    Func = async (issue, oldState, newState) => await SetState(issue, newState)
                });
            _updateStateHandler.AddEvent(
                new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //NegotiationState -> ProcessingState
                {
                    OldState = State.NegotiationState,
                    NewState = State.ProcessingState,
                    Func = moveToProcessingPhase
                });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //BlockedState -> ProcessingState
            {
                OldState = State.BlockedState,
                NewState = State.ProcessingState,
                Func = moveToProcessingPhase
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //WaitingState -> ProcessingState
            {
                OldState = State.WaitingState,
                NewState = State.ProcessingState,
                Func = moveToProcessingPhase
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //Cancel Issue
            {
                OldState = "",
                NewState = State.CancelledState,
                Func = async (issue, oldState, newState) =>
                {
                    //Cancels all children
                    await SetState(issue, newState);
                    var children = await _issueHelper.GetChildrenRecursive(issue);
                    await Task.WhenAll(children.Select(it => UpdateState(it, newState)));
                    await OnIssueReachedCompletionPhase(issue);
                    return newState;
                }
            });
            _updateStateHandler.AddEvent(
                new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //ProcessingState -> ReviewState
                {
                    OldState = State.ProcessingState,
                    NewState = State.ReviewState,
                    Func = async (issue, oldState, newState) =>
                    {
                        var children = await Task.WhenAll(issue.ChildrenIssueIds.Select(_issueRepository.GetAsync));
                        var childrenStates = await Task.WhenAll(children.Select(it => _stateService.GetState(it.ProjectId, it.StateId)));
                        if (childrenStates.HasWhere(it => it.Phase != State.ConclusionPhase))
                            throw new HttpStatusException(400, "At least one sub issue is not in conclusion phase");
                        return await SetState(issue, newState);
                    }
                });
            _updateStateHandler.AddEvent(
                new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //ReviewState -> CompletedState
                {
                    OldState = State.ReviewState,
                    NewState = State.CompletedState,
                    Func = async (issue, oldState, newState) =>
                    {
                        //TODO Projektleiter überprüft ticket
                        await SetState(issue, newState);
                        await OnIssueReachedCompletionPhase(issue);
                        return newState;
                    }
                });
            _updateStateHandler.AddEvent(
                new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //CompletedState -> ArchivedState
                {
                    OldState = State.CompletedState,
                    NewState = State.ArchivedState,
                    Func = async (issue, oldState, newState) =>
                    {
                        //TODO Abschlusskommentar
                        await SetState(issue, newState);
                        await OnIssueReachedCompletionPhase(issue);
                        return newState;
                    }
                });
        }

        private void RegisterPossibleUsergeneratedStates()
        {
            _updateStateHandler.AddUserGeneratedStateCase(State.NegotiationPhase, State.NegotiationState);
            _updateStateHandler.AddUserGeneratedStateCase(State.ProcessingPhase, State.ProcessingState);
            _updateStateHandler.AddUserGeneratedStateCase(State.ConclusionPhase, State.CompletedState);
        }

        private async Task OnIssueReachedCompletionPhase(Issue issue, Issue? _parent = null, IList<Issue>? _successors = null)
        {
            var successors = _successors == null
                ? Task.WhenAll(issue.SuccessorIssueIds.Select(_issueRepository.GetAsync))
                : Task.FromResult(_successors.ToArray());

            try
            {
                var processingState = await GetStateByName(issue, State.ProcessingState);
                var blockedState = await GetStateByName(issue, State.BlockedState);
                var blockedSuccessors = (await successors).Where(it => it.StateId == blockedState.Id).ToList();

                await Task.WhenAll(blockedSuccessors.Select(it => TryCatch(UpdateState(it, processingState))));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /*
         * Gibt den neuen Status zurück (schmeißt eine Exception wenn der Status nicht geändert werden kann).
         * Updated Issue und alle abhängigen Issues
         */
        public async Task<StateDTO> UpdateState(Issue issue, StateDTO newState)
        {
            if (issue.StateId == newState.Id) return newState;
            var state = await _updateStateHandler.HandleStateUpdate(issue, await _stateService.GetState(issue.ProjectId, issue.StateId), newState);
            return state;
        }

        private async Task<StateDTO> SetState(Issue issue, StateDTO state)
        {
            issue.StateId = state.Id;
            await _issueRepository.UpdateAsync(issue);
            return state;
        }

        /*
         * Gibt den neuen Status zurück (schmeißt eine Exception wenn der Status nicht geändert werden kann).
         * Der Status von jeden abhängigem Issue wird ebenfalls geupdated 
         */
        private async Task<StateDTO> GetStateByName(Issue issue, string name)
        {
            return (await _stateService.GetStates(issue.ProjectId)).First(it => it.Name == name);
        }

        private async Task TryCatch(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
            }
        }
    }
}