using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.projects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class Issue : Document
    {
        
        [BsonElement("state_id")] 
        public ObjectId StateId { get; set; }
        
        [BsonElement("project_id")] 
        public ObjectId ProjectId { get; set; }
        
        [BsonElement("client_id")] 
        public ObjectId ClientId { get; set; }
        
        [BsonElement("author_id")] 
        public ObjectId AuthorId { get; set; }
        
        [BsonElement("assignedUsers_id")] 
        public IList<ObjectId> AssignedUserIds { get; set; }

        [BsonElement("conversationItems")] 
        public IList<IssueConversation> ConversationItems { get; set; }
        
        [BsonElement("timeSheets")] 
        public IList<TimeSheet> TimeSheets { get; set; }
        
        [BsonElement("IssueDetail")] 
        public IssueDetail IssueDetail { get; set; }
        
        [BsonElement("parentIssue_id")] 
        public ObjectId ParentIssueId { get; set; }
        
        [BsonElement("predecessor_issue_ids")] 
        public IList<ObjectId> PredecessorIssueIds { get; set; }
        
        [BsonElement("successor_issue_ids")] 
        public IList<ObjectId> SuccessorIssueIds { get; set; }
    }
}