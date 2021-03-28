using System;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Tickets;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.Issues
{
    public class IssueTimeSheetDTO
    {
        public IssueTimeSheetDTO(TimeSheet timeSheet, User user)
        {
            Id = timeSheet.Id;
            Start = timeSheet.Start;
            End = timeSheet.End;
            User = new UserDTO(user);
        }

        public ObjectId Id { get; set; }
        public UserDTO User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}