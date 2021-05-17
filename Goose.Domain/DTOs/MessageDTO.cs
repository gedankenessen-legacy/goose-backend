using Goose.Domain.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class MessageDTO
    {
        public ObjectId Id { get; set; }
        public ObjectId CompanyId { get; set; }
        public ObjectId ProjectId { get; set; }
        public ObjectId IssueId { get; set; }
        public ObjectId ReceiverUserId { get; set; }
        public MessageType Type { get; set; }
        public bool Consented { get; set; }

        public MessageDTO()
        {

        }

        public MessageDTO(Message message)
        {
            Id = message.Id;
            CompanyId = message.CompanyId;
            ProjectId = message.ProjectId;
            IssueId = message.IssueId;
            ReceiverUserId = message.ReceiverUserId;
            Type = message.Type;
            Consented = message.Consented;
        }
    }
}
