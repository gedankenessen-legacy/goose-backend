using MongoDB.Bson;

namespace Goose.Domain.Models.tickets
{
    public class IssueRequirement
    {
        public ObjectId Id { get; set; }
        public string Requirement { get; set; }
    }
}