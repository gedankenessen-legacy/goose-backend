using System;
using Goose.Domain.Models.identity;
using MongoDB.Bson;

namespace Goose.Domain.Models.tickets
{
    public class TimeSheet
    {
        public User User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}