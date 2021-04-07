using System;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Tickets
{
    public class TimeSheet
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}