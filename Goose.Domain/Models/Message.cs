using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models
{
    public class Message
    {
        
        [BsonElement("receiver_user_id")] 
        public ObjectId ReceiverUserId { get; set; }
        
        [BsonElement("type")] 
        public string Type { get; set; }
        
        [BsonElement("consented")] 
        public bool Consented { get; set; }
        
        [BsonElement("data")] 
        public object Data { get; set; }
    }
}