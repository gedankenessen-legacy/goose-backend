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
        public const string CheckingState = "Überprüfung";
        public const string NegotiationState = "Verhandlung";

        public const string BlockedState = "Blockiert";
        public const string WaitingState = "Wartend";
        public const string ProcessingState = "Bearbeiten";
        public const string ReviewState = "Review";

        public const string CompletedState = "Abgeschlossen";
        public const string CancelledState = "Abgebrochen";
        public const string ArchivedState = "Archiviert";

        // Dies sind die gültigen Werte für die Phasen
        public const string NegotiationPhase = "Verhandlungsphase";
        public const string ProcessingPhase = "Bearbeitungsphase";
        public const string ConclusionPhase = "Abschlussphase";
    }
}