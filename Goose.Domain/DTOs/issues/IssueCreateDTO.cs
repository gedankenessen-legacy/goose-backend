using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.issues
{
    public class IssueCreateDTO
    {
        [Required] public ObjectId StateId { get; set; }
        [Required] public ObjectId ProjectId { get; set; }
        [Required] public ObjectId ClientId { get; set; }
        [Required] public ObjectId AuthorId { get; set; }
        [Required] public IssueDetail IssueDetail { get; set; }

        public Issue ToIssue()
        {
            return new Issue
            {
                StateId = StateId,
                ProjectId = ProjectId,
                ClientId = ClientId,
                AuthorId = AuthorId,
                IssueDetail = IssueDetail,
                ConversationItems = new List<IssueConversation>(),
                TimeSheets = new List<TimeSheet>(),
                AssignedUserIds = new List<ObjectId>(),
                ParentIssueId = null,
                PredecessorIssueIds = new List<ObjectId>(),
                SuccessorIssueIds = new List<ObjectId>()
            };
        }
    }
}