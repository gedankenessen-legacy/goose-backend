using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Projects
{
    public class State
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
        public bool UserGenerated { get; set; }
    }
}