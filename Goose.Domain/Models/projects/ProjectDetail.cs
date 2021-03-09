using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.projects
{
    public class ProjectDetail
    {
        [BsonElement("name")] 
        public string Name { get; set; }
    }
}