using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueRequirement
    {
        public ObjectId id { get; set; }
        public string Requirement { get; set; }
    }
}