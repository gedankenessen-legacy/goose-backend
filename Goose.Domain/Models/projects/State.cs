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

        // Dies sind die gültigen Werte für die Phasen
        public const string NegotiationPhase = "Negotiation";
        public const string ProcessingPhase = "Processing";
        public const string ConclusionPhase = "Conclusion";
    }
}