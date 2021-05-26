using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Goose.API.Repositories;
using Goose.API.Utils;
using Goose.API.Utils.Exceptions;
using Goose.Domain.DTOs;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;

namespace Goose.API.Services.issues
{
    public interface IIssueStateService
    {
        public Task<StateDTO> UpdateState(Issue issue, StateDTO newState);
        public Task<StateDTO> GetNewStateUpdateAssociatedIssues(Issue issue, StateDTO newState);
    }

    public class IssueStateService : IIssueStateService
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IStateService _stateService;
        private readonly IIssueAssociationHelper _associationHelper;

        private readonly IFsmHandler<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>, StateDTO> _updateStateHandler = new FsmHandler();

        public IssueStateService(IIssueRepository issueRepository, IStateService stateService, IIssueAssociationHelper associationHelper)
        {
            _issueRepository = issueRepository;
            _stateService = stateService;
            _associationHelper = associationHelper;

            RegisterFsmEvents();
        }

        public void RegisterFsmEvents()
        {
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //CheckingState -> NegotiationState
            {
                OldState = State.CheckingState,
                NewState = State.NegotiationState,
                Func = async (issue, oldState, newState) => newState
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //NegotiationState -> ProcessingState
            {
                OldState = State.NegotiationState,
                NewState = State.ProcessingState,
                Func = async (issue, oldState, newState) =>
                {
                    //TODO nur wenn zusammenfassung akzeptiert ist

                    /*
                     * Das Issue wird in Waiting gesetzt wenn das Startdatum noch nicht erreicht ist
                     */
                    if (issue.IssueDetail.StartDate < DateTime.Now)
                    {
                        return await GetStateByName(issue, State.WaitingState);
                    }

                    Task<Issue> parent = null;
                    if (issue.ParentIssueId is { } parentId) parent = _issueRepository.GetAsync(parentId);
                    var predecessors = Task.WhenAll(issue.PredecessorIssueIds.Select(_issueRepository.GetAsync));


                    Task<StateDTO> parentState = null;
                    if (parent is { } parentNotNull) parentState = _stateService.GetState((await parentNotNull).ProjectId, (await parentNotNull).Id);
                    var predecessorStates = Task.WhenAll((await predecessors).Select(it => _stateService.GetState(it.ProjectId, it.StateId)));

                    /*
                     * Issue wird blockiert wenn:
                     * 1) Das Oberticket nicht in der Bearbeitungsphase ist
                     * 2) Nicht alle Vorgänger abgeschlossen sind
                     * 
                     */
                    if (parentState != null && (await parentState).Phase == State.NegotiationPhase ||
                        (await predecessorStates).HasWhere(it => it.Phase != State.ConclusionPhase))
                    {
                        return await GetStateByName(issue, State.BlockedState);
                    }

                    return newState;
                }
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //Cancel Issue
            {
                OldState = "",
                NewState = State.CancelledState,
                Func = async (issue, oldState, newState) =>
                {
                    //Cancels all children
                    var children = await _associationHelper.GetChildrenRecursive(issue);
                    await Task.WhenAll(children.Select(it => UpdateState(it, newState)));
                    //TODO was ist mit nachfolgern
                    return newState;
                }
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //CheckingState -> NegotiationState
            {
                OldState = State.CheckingState,
                NewState = State.NegotiationState,
                Func = async (issue, oldState, newState) => newState
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //ProcessingState -> ReviewState
            {
                OldState = State.ProcessingState,
                NewState = State.ReviewState,
                Func = async (issue, oldState, newState) =>
                {
                    var children = await Task.WhenAll(issue.ChildrenIssueIds.Select(_issueRepository.GetAsync));
                    var childrenStates = await Task.WhenAll(children.Select(it => _stateService.GetState(it.ProjectId, it.StateId)));
                    if (childrenStates.HasWhere(it => it.Phase != State.ConclusionPhase))
                        throw new HttpStatusException(400, "At least one sub issue is not in conclusion phase");
                    return newState;
                }
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //ReviewState -> CompletedState
            {
                OldState = State.ReviewState,
                NewState = State.CompletedState,
                Func = async (issue, oldState, newState) =>
                {
                    //TODO Projektleiter überprüft ticket
                    await OnIssueReachedCompletionPhase(issue);
                    return newState;
                }
            });
            _updateStateHandler.AddEvent(new FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>> //CompletedState -> ArchivedState
            {
                OldState = State.CompletedState,
                NewState = State.ArchivedState,
                Func = async (issue, oldState, newState) =>
                {
                    //TODO Abschlusskommentar
                    await OnIssueReachedCompletionPhase(issue);
                    return newState;
                }
            });
        }

        private async Task OnIssueReachedCompletionPhase(Issue issue, Issue? _parent = null, IList<Issue>? _successors = null)
        {
            Task<Issue> parentAsync = null;
            if (_parent != null) parentAsync = Task.FromResult(_parent);
            else if (issue.ParentIssueId is { } parentId) parentAsync = _issueRepository.GetAsync(parentId);

            var successors = _successors == null
                ? Task.WhenAll(issue.SuccessorIssueIds.Select(_issueRepository.GetAsync))
                : Task.FromResult(_successors.ToArray());

            try
            {
                var processingState = await GetStateByName(issue, State.ProcessingState);
                var blockedState = await GetStateByName(issue, State.BlockedState);
                var tasks = new List<Task>();
                var parent = parentAsync != null ? await parentAsync : null;
                if (parent is { } parentNotNull)
                {
                    if (parentNotNull.StateId == blockedState.Id)
                        tasks.Add(TryCatch(UpdateState(parentNotNull, processingState)));
                }

                var blockedSuccessors = (await successors).Where(it => it.StateId == blockedState.Id);
                tasks.AddRange(blockedSuccessors.Select(it => TryCatch(UpdateState(it, processingState))));

                await Task.WhenAll(tasks);
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
            var state = await GetNewStateUpdateAssociatedIssues(issue, newState);
            issue.StateId = state.Id;
            await _issueRepository.UpdateAsync(issue);
            return state;
        }

        /*
         * Gibt den neuen Status zurück (schmeißt eine Exception wenn der Status nicht geändert werden kann).
         * Der Status von jeden abhängigem Issue wird ebenfalls geupdated 
         */
        public async Task<StateDTO> GetNewStateUpdateAssociatedIssues(Issue issue, StateDTO newState)
        {
            return await _updateStateHandler.HandleStateUpdate(issue, await _stateService.GetState(issue.ProjectId, issue.StateId), newState);
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

        private async Task<StateDTO> GetStateByName(Issue issue, string name)
        {
            return (await _stateService.GetStates(issue.ProjectId)).First(it => it.Name == name);
        }

        private StateDTO GetStateFromList(ObjectId id, IList<StateDTO> states) => states.First(it => it.Id == id);

        private async Task<StateDTO> GetState(Issue issue)
        {
            return await _stateService.GetState(issue.ProjectId, issue.StateId);
        }
    }

    class FsmHandler : IFsmHandler<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>, StateDTO>
    {
        private List<FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>>> _events1 = new();

        List<FsmEvent<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>>> IFsmHandler<Func<Issue, StateDTO, StateDTO, Task<StateDTO>>, StateDTO>._events
        {
            get => _events1;
            set => _events1 = value;
        }

        public async Task<StateDTO> CallAction(Func<Issue, StateDTO, StateDTO, Task<StateDTO>> action, Issue issue, StateDTO oldState, StateDTO newState)
        {
            return await action(issue, oldState, newState);
        }
    }

    interface IFsmHandler<TFunc, TReturn>
    {
        protected List<FsmEvent<TFunc>> _events { get; set; }

        public bool AddEvent(FsmEvent<TFunc> e)
        {
            if (Contains(e)) return false;
            _events.Add(e);
            return true;
        }

        public Task<TReturn> CallAction(TFunc action, Issue issue, StateDTO oldState, StateDTO newState);

        public async Task<TReturn> HandleStateUpdate(Issue issue, StateDTO oldState, StateDTO newState)
        {
            if (issue == null || oldState == null || newState == null)
                throw new HttpStatusException(400, "Invalid request, Issue or State does not exist");
            var e = Find(oldState.Name, newState.Name);
            if (e == null) throw new HttpStatusException(400, $"cannot update an issue state from {oldState.Name} to {newState.Name}");
            return await CallAction(e.Func, issue, oldState, newState);
        }

        private bool Contains(FsmEvent<TFunc> e) => Find(e.OldState, e.NewState) != null;

        private FsmEvent<TFunc>? Find(string oldState, string newState)
        {
            var e = new FsmEvent<TFunc>(oldState, newState, default);
            return _events.FirstOrDefault(it => Equals(it, e));
        }

        private bool Equals(FsmEvent<TFunc> first, FsmEvent<TFunc> second) =>
            //((first.OldState == "" || second.OldState == "") || first.OldState == second.OldState) && first.NewState == second.NewState;
            first.OldState + second.OldState == second.OldState + first.OldState && first.NewState == second.NewState;
    }

    class FsmEvent<TFunc>
    {
        public FsmEvent(string oldState, string newState, TFunc func)
        {
            OldState = oldState;
            NewState = newState;
            Func = func;
        }

        public FsmEvent()
        {
        }

        public string OldState { get; set; }
        public string NewState { get; set; }
        public TFunc Func { get; set; }
    }
}