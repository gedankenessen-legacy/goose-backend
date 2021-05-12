using Goose.Data.Models;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models
{
    public class Message : Document
    {
        public ObjectId CompanyId { get; set; }
        public ObjectId ProjectId { get; set; }
        public ObjectId IssueId { get; set; }
        public ObjectId ReceiverUserId { get; set; }
        public MessageType Type { get; set; }
        public bool Consented { get; set; }
    }

    public enum MessageType
    {
        TimeSkipped,
        TimeExceeded,
        IssueCancelled,
        RecordedTimeChanged,
        NewConversationItem
    }
}