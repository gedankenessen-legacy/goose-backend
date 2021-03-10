using System;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class TimeSheet
    {
        public ObjectId UserId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}