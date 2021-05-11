using Goose.Domain.Models.Issues;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Goose.Domain.DTOs.Issues
{
    public class IssueConversationDTO
    {
        public ObjectId Id { get; set; }
        public UserDTO Creator { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public ObjectId? OtherTicketId { get; set; }
        public IList<string> Requirements { get; set; }
        public DateTime CreatedAt { get; set; }

        public IssueConversationDTO() { }

        public IssueConversationDTO(IssueConversation issueConversation, UserDTO creator)
        {
            Id = issueConversation.Id;
            Creator = creator;
            Type = issueConversation.Type;
            Data = issueConversation.Data;
            OtherTicketId = issueConversation.OtherTicketId;
            Requirements = issueConversation.Requirements;
            CreatedAt = issueConversation.CreatedAt;
        }
    }
}
