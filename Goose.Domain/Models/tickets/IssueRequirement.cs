using MongoDB.Bson;

namespace Goose.Domain.Models.tickets
{
    public class IssueRequirement
    {
        public ObjectId _id { get; set; }
        public string Requirement { get; set; }
    }
}