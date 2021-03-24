using System.Collections.Generic;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.projects;
using Goose.Domain.Models.tickets;

namespace Goose.Domain.DTOs.issues
{
    public class IssueDTODetailed
    {
        public State State { get; set; }
        public Project Project { get; set; }
        public User Client { get; set; }
        public User Author { get; set; }
        public IList<User> AssignedUsers { get; set; }
        public IList<IssueConversation> ConversationItems { get; set; }
        public IList<TimeSheet> TimeSheets { get; set; }
        public IssueDetail IssueDetail { get; set; }
        public Issue ParentIssue { get; set; }
        public IList<Issue> PredecessorIssues { get; set; }
        public IList<Issue> SuccessorIssues { get; set; }
    }
}