using System;
using System.Collections.Generic;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueConversation
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId? CreatorUserId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public IList<ObjectId> RequirementIds { get; set; }
        public DateTime CreatedAt { get => Id.CreationTime; }


        public const string MessageType = "Nachricht";
        public const string StateChangeType = "Statusänderung";
        public const string SummaryCreatedType = "Zusammenfassung";
        public const string SummaryAcceptedType = "Zusammenfassung akzeptiert";
        public const string SummaryDeclinedType = "Zusammenfassung abgelehnt";
        public const string PredecessorAddedType = "Vorgänger hinzugefügt";
        public const string PredecessorRemovedType = "Vorgänger entfernt";
        public const string ChildIssueAddedType = "Unterticket hinzugefügt";
        // TODO brauchen wir das?
        public const string ChildIssueRemovedType = "Unterticket entfernt";
    }
}