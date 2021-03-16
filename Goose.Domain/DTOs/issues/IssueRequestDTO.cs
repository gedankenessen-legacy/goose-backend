using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.issues
{
    public class IssueRequestDTO
    {
        public ObjectId StateId { get; set; }
        public ObjectId ProjectId { get; set; }
        public ObjectId ClientId { get; set; }
        public ObjectId AuthorId { get; set; }
        public IssueDetail IssueDetail { get; set; }
    }
}