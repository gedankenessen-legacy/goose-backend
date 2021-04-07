using MongoDB.Bson;

namespace Goose.Domain.Models.Tickets
{
    public class IssueRequirement
    {
        public ObjectId Id { get; set; }
        public string Requirement { get; set; }
    }
}