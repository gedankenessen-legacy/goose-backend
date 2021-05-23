using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Projects;
using MongoDB.Bson;

namespace Goose.Domain.Models.Issues
{
    public class Issue : Document
    {
        public const string TypeBug = "bug";
        public const string TypeFeature = "feature";

        public ObjectId StateId { get; set; }
        public ObjectId ProjectId { get; set; }
        public ObjectId ClientId { get; set; }
        public ObjectId AuthorId { get; set; }
        public IList<ObjectId> AssignedUserIds { get; set; }
        public IList<IssueConversation> ConversationItems { get; set; }
        public IList<TimeSheet> TimeSheets { get; set; }
        public IssueDetail IssueDetail { get; set; }
        public ObjectId? ParentIssueId { get; set; }
        public IList<ObjectId> ChildrenIssueIds { get; set; }
        public IList<ObjectId> PredecessorIssueIds { get; set; }
        public IList<ObjectId> SuccessorIssueIds { get; set; }
    }
}