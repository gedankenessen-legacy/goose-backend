using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.projects
{
    public class State
    {
        [BsonElement("_id")] 
        public ObjectId _id { get; set; }
        
        [BsonElement("name")] 
        public string Name { get; set; }
        
        [BsonElement("phase")] 
        public string Phase { get; set; }
        
        [BsonElement("userGenerated")] 
        public bool UserGenerated { get; set; }
    }
}