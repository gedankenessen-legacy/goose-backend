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
        public IList<IssueRequirement> Requirements { get; set; }
        public DateTime CreatedAt { get; set; }

        public IssueConversationDTO() { }

        public IssueConversationDTO(IssueConversation issueConversation, UserDTO creator, IList<IssueRequirement> requirements)
        {
            Id = issueConversation.Id;
            Creator = creator;
            Type = issueConversation.Type;
            Data = issueConversation.Data;
            Requirements = requirements;
            CreatedAt = issueConversation.CreatedAt;
        }
    }
}
