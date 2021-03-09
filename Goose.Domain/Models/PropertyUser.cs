using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models
{
    public class PropertyUser
    {
        [BsonElement("_id")] 
        public ObjectId _id { get; set; }
        
        [BsonElement("user_id")] 
        public ObjectId UserId { get; set; }
        
        [BsonElement("role_ids")] 
        public IList<ObjectId> RoleIds { get; set; }
    }
}