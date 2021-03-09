using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueRequirement
    {
        [BsonElement("_id")] 
        public ObjectId _id { get; set; }
        
        [BsonElement("requirement")] 
        public string Requirement { get; set; }
    }
}