#nullable enable
using System.Collections.Generic;
using Goose.Domain.DTOs.Tickets;
using Goose.Domain.Models.Tickets;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.Issues
{
    public class IssueDTODetailed
    {
        public ObjectId? Id { get; set; }
        public StateDTO State { get; set; }
        public ProjectDTO Project { get; set; }
        public UserDTO Client { get; set; }
        public UserDTO Author { get; set; }
        public IList<UserDTO>? AssignedUsers { get; set; }
        public IList<IssueConversationDTO>? ConversationItems { get; set; }
        public IList<IssueTimeSheetDTO>? TimeSheets { get; set; }
        public IssueDetail IssueDetail { get; set; }
        public IssueDTO? ParentIssue { get; set; }
        public IList<IssueDTO>? PredecessorIssues { get; set; }
        public IList<IssueDTO>? SuccessorIssues { get; set; }

        public IssueDTODetailed()
        {
        }

        public IssueDTODetailed(ObjectId id, StateDTO state, ProjectDTO project, UserDTO client, UserDTO author,
            IList<UserDTO>? assignedUsers, IList<IssueConversationDTO>? conversationItems,
            IList<IssueTimeSheetDTO>? timeSheets, IssueDetail issueDetail, IssueDTO? parentIssue,
            IList<IssueDTO>? predecessorIssues, IList<IssueDTO>? successorIssues)
        {
            Id = id;
            State = state;
            Project = project;
            Client = client;
            Author = author;
            AssignedUsers = assignedUsers;
            ConversationItems = conversationItems;
            TimeSheets = timeSheets;
            IssueDetail = issueDetail;
            ParentIssue = parentIssue;
            PredecessorIssues = predecessorIssues;
            SuccessorIssues = successorIssues;
        }
    }
}