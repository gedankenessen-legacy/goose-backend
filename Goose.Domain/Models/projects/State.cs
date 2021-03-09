using MongoDB.Bson;

namespace Goose.Domain.Models.projects
{
    public class State
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
        public bool UserGenerated { get; set; }
    }
}