using Goose.API.Services;
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
    public sealed class TicketDeadlineEvent : IEvent
    {
        private static readonly object _runningDeadlinesLock = new object();
        private static readonly Dictionary<ObjectId, EventCanceller> _runningDeadlines = new Dictionary<ObjectId, EventCanceller>();
        private IMessageService _messageService;

        public Issue Issue { get; }
        public Project Project { get; }
        public DateTime Time { get; }

        public TicketDeadlineEvent(Project project, Issue issue, IMessageService messageService)
        {
            Project = project;
            Time = (DateTime)issue.IssueDetail.EndDate;
            Issue = issue;
            _messageService = messageService;
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
            // This deadline is not running anymore, clean up from dictionary
            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.Remove(Issue.Id);
            }

            // TODO
            // Look for Ticket in DB:
            // Is the deadline still the same?

            // If yes, stop the ticket, generate messages
            await CreateDeadLineReachedMessage();
        }

        public Task OnCancelled()
        {
            // This deadline is not running anymore, clean up from dictionary
            lock (_runningDeadlinesLock)
            {
                _runningDeadlines.Remove(Issue.Id);
            }
            return Task.CompletedTask;
        }

        private async Task CreateDeadLineReachedMessage()
        {
            await CreateDeadLineReachedMessage(Project.CompanyId, Project.Id, Issue.Id, Issue.AuthorId);
            await CreateDeadLineReachedMessage(Project.CompanyId, Project.Id, Issue.Id, Issue.ClientId);
        }

        private async Task CreateDeadLineReachedMessage(ObjectId companyId, ObjectId projectId, ObjectId issueId, ObjectId userId)
        {
            await _messageService.CreateMessageAsync(new Message()
            {
                CompanyId = companyId,
                ProjectId = projectId,
                IssueId = issueId,
                ReceiverUserId = userId,
                Type = MessageType.DeadLineReached,
                Consented = false
            });
        }
    }
}
