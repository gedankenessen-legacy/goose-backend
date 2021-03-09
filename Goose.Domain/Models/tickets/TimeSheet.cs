using System;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class TimeSheet
    {
        [BsonElement("user_id")] 
        public ObjectId UserId { get; set; }
        
        [BsonElement("start")] 
        public DateTime Start { get; set; }
        
        [BsonElement("end")] 
        public DateTime End { get; set; }
    }
}