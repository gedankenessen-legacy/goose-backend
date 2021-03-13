using System;
using System.Collections.Generic;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueConversation
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId CreatorUserId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public IList<ObjectId> RequirementIds { get; set; }
        public DateTime CreatedAt { get => Id.CreationTime; }
    }
}