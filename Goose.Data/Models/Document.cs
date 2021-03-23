using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Text.Json.Serialization;

namespace Goose.Data.Models
{
    public interface IDocument
    {
        ObjectId Id { get; set; }
        DateTime CreatedAt { get; }
    }

    public class Document : IDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime CreatedAt => Id.CreationTime;
    }
}
