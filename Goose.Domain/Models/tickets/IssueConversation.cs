using System;
using System.Collections.Generic;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueConversation
    {
        
        [BsonElement("_id")] 
        public ObjectId _id { get; set; }
        
        [BsonElement("creator_user_id")] 
        public ObjectId CreatorUserId { get; set; }
        
        [BsonElement("type")] 
        public string Type { get; set; }
        
        [BsonElement("data")] 
        public object Data { get; set; }
        
        [BsonElement("requirements_id")] 
        public IList<ObjectId> RequirementIds { get; set; }
        
        [BsonElement("createdAt")] 
        public DateTime CreatedAt { get; set; }
    }
}