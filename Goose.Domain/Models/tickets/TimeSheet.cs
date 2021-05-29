using System;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Issues
{
    public class TimeSheet
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Start { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime End { get; set; }
    }
}