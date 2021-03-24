using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.projects
{
    public class State
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
        public bool UserGenerated { get; set; }

        // Dies sind die Namen für die Vordefinierten Status
        public const string CheckingState = "Checking";
        public const string NegotiationState = "Negotiation";
        public const string BlockedState = "Blocked";
        public const string WaitingState = "Waiting";
        public const string ProcessingState = "Processing";
        public const string ReviewState = "Review";
        public const string CompletedState = "Completed";
        public const string CancelledState = "Cancelled";
        public const string ArchivedState = "Archived";

        // Dies sind die gültigen Werte für die Phasen
        public const string NegotiationPhase = "Negotiation";
        public const string ProcessingPhase = "Processing";
        public const string ConclusionPhase = "Conclusion";
    }
}