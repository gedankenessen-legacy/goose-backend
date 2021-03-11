using Goose.Domain.Models.tickets;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs.tickets
{
    public class IssueConversationDTO
    {
        public ObjectId Id { get; set; }
        public UserDTO Creator { get; set; }
        public string Type { get; set; }
        public object Data { get; set; }
        public IList<IssueRequirementDTO> Requirements { get; set; }
        public DateTime CreatedAt { get; set; }

        public IssueConversationDTO(IssueConversation issueConversation, UserDTO creator, IList<IssueRequirementDTO> requirements)
        {
            Id = issueConversation.Id;
            Creator = creator;
            Type = issueConversation.Type;
            Data = issueConversation.Data;
            Requirements = requirements;
            CreatedAt = issueConversation.CreatedAt;
        }

        //TODO: Remove as soon the real classes are created
        public class IssueRequirementDTO {}
        public class UserDTO {}
    }
}
