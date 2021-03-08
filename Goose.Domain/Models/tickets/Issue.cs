﻿using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.identity;
using Goose.Domain.Models.projects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class Issue : Document
    {
        public ObjectId StateId { get; set; }
        public ObjectId ProjectId { get; set; }
        public ObjectId ClientId { get; set; }
        public ObjectId AuthorId { get; set; } 
        public IList<ObjectId> AssignedUserIds { get; set; }
        public IList<IssueConversation> ConversationItems { get; set; }
        public IList<TimeSheet> TimeSheets { get; set; }
        public IssueDetail IssueDetail { get; set; }
        public ObjectId ParentIssueId { get; set; }
        public IList<ObjectId> PredecessorIssueIds { get; set; }
        public IList<ObjectId> SuccessorIssueIds { get; set; }
    }
}