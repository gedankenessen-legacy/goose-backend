using System;
using Goose.Domain.Models.Identity;
using Goose.Domain.Models.Issues;
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

        public TimeSheet ToTimeSheet()
        {
            return new()
            {
                End = End,
                Id = Id,
                Start = Start,
                UserId = User.Id
            };
        }

        public ObjectId Id { get; set; }
        public UserDTO User { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}