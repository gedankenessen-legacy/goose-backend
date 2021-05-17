using System;
using System.Collections.Generic;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Goose.Domain.Models.Issues
{
    public class IssueConversation
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId CreatorUserId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }

        // This field is used to store a reference to the added/removed predecessor/child
        public ObjectId? OtherTicketId { get; set; }

        // This field is used for the state changes
        public StateChange StateChange { get; set; }

        // These one are used for the requirement messages
        public IList<string> Requirements { get; set; }
        public double? ExpectedTime { get; set; }

        public DateTime CreatedAt { get => Id.CreationTime; }


        public const string MessageType = "Nachricht";
        public const string StateChangeType = "Statusänderung";
        public const string SummaryCreatedType = "Zusammenfassung";
        public const string SummaryAcceptedType = "Zusammenfassung akzeptiert";
        public const string SummaryDeclinedType = "Zusammenfassung abgelehnt";
        public const string PredecessorAddedType = "Vorgänger hinzugefügt";
        public const string PredecessorRemovedType = "Vorgänger entfernt";
        public const string ChildIssueAddedType = "Unterticket hinzugefügt";
        public const string ChildIssueRemovedType = "Unterticket entfernt";
    }

    public class StateChange
    {
        public string Before { get; set; }
        public string After { get; set; }
    }
}