using Goose.Data.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.identity
{
    public class User: Document
    {
        [BsonElement("firstname")] 
        public string Firstname { get; set; }
        
        [BsonElement("lastname")] 
        public string Lastname { get; set; }
        
        [BsonElement("hashedPassword")] 
        public string HashedPassword { get; set; }
    }
}