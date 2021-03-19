using System;
using System.Collections.Generic;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueConversation
    {
        public ObjectId id { get; set; }
        public ObjectId CreatorUserId { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        public IList<ObjectId> RequirementIds { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}