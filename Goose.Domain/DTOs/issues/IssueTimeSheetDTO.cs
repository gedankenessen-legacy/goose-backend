using System;
using Goose.Domain.Models.identity;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.issues
{
    public class IssueTimeSheetDTO
    {
        public ObjectId Id { get; set; }
        public UserDTO User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}