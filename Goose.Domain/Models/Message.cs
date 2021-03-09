using Goose.Domain.Models.identity;
using MongoDB.Bson;

namespace Goose.Domain.Models
{
    public class Message
    {
        public ObjectId ReceiverUserId { get; set; }
        public string Type { get; set; }
        public bool Consented { get; set; }
        public object Data { get; set; }
    }
}