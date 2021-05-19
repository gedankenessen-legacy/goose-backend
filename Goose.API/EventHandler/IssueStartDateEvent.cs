using Goose.API.Repositories;
using Goose.API.Services;
using Goose.Domain.DTOs;
using Goose.Domain.Models;
using Goose.Domain.Models.Issues;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.EventHandler
{
    public class IssueStartDateEvent : IEvent
    {
        private static readonly object _runningDeadlinesLock = new object();
        private static readonly Dictionary<ObjectId, EventCanceller> _runningDeadlines = new Dictionary<ObjectId, EventCanceller>();
        private readonly IIssueRepository _issueRepository;
        private readonly IStateService _stateService;

        public DateTime Time { get; }
        public Issue Issue { get; }

        public IssueStartDateEvent(Issue issue, IIssueRepository issueRepository, IStateService stateService)
        {
            Time = (DateTime)issue.IssueDetail.StartDate;
            Issue = issue;
            _issueRepository = issueRepository;
            _stateService = stateService;
        }

        public Task OnCancelled()
        {
            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.Remove(Issue.Id);
            }
            return Task.CompletedTask;
        }

        public static async Task CancelDeadLine(ObjectId issueId)
        {
            EventCanceller? oldCanceller;

            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.TryGetValue(issueId, out oldCanceller);
            }

            // old canceller cannot be awaited in the lock
            if (oldCanceller != null)
                await oldCanceller.Cancel();
        }

        public async Task OnStarted(EventExecutor executor)
        {
            EventCanceller? oldCanceller;

            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.TryGetValue(Issue.Id, out oldCanceller);
            }

            // old canceller cannot be awaited in the lock
            if (oldCanceller != null)
            {
                await oldCanceller.Cancel();
            }

            lock (_runningDeadlinesLock)
            {
                // Get a canceller from the EventExecutor and update the index
                _runningDeadlines[Issue.Id] = executor.GetEventCanceller();
            }
        }

        public async Task OnTimeReached()
        {
            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.Remove(Issue.Id);
            }
            await UpdateIssue();
        }

        private async Task UpdateIssue()
        {
            var issue = await _issueRepository.GetAsync(Issue.Id);
           
            if (issue is null)
                return;

            var state = await GetState(issue.ProjectId, State.ProcessingState);

            issue.StateId = state.Id;

            await _issueRepository.UpdateAsync(issue);
        }

        private async Task<StateDTO> GetState(ObjectId projectId, string stateName)
        {
            var states = await _stateService.GetStates(projectId);
            var state = states.FirstOrDefault(it => it.Name.Equals(stateName));
            return state;
        }
    }
}
