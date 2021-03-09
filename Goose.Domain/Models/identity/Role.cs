using Goose.Data.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.identity
{
    public class Role: Document
    {
        [BsonElement("name")] 
        public string Name { get; set; }
    }
}