using Goose.Domain.Models.tickets;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Goose.Domain.DTOs.tickets
{
    public class IssueConversationDTO
    {
        public string Id { get; set; }
        public UserDTO Creator { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        public IList<IssueRequirementDTO> Requirements { get; set; }
        public DateTime CreatedAt { get; set; }

        public IssueConversationDTO() { }

        public IssueConversationDTO(IssueConversation issueConversation, UserDTO creator, IList<IssueRequirementDTO> requirements)
        {
            Id = issueConversation.Id.ToString();
            Creator = creator;
            Type = issueConversation.Type;
            Data = issueConversation.Data;
            Requirements = requirements;
            CreatedAt = issueConversation.CreatedAt;
        }

        //TODO: Remove as soon the real classes are created
        public class IssueRequirementDTO { public string Id { get; set; } }
        public class UserDTO { public string Id { get; set; } }
    }
}
