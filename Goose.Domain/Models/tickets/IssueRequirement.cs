using MongoDB.Bson;

namespace Goose.Domain.Models.Issues
{
    public class IssueRequirement
    {
        public ObjectId Id { get; set; }
        public string Requirement { get; set; }
        public bool Achieved { get; set; }
    }
}