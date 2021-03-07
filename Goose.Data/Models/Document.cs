using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace Goose.Data.Models
{
    public interface IDocument
    {
        string Id { get; set; }
        DateTime CreatedAt { get; }
    }

    public class Document : IDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("createdAt")]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt => (Id != null) ? ObjectId.Parse(Id).CreationTime : default;
    }
}
