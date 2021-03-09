using System.Collections.Generic;
using System.Text.Json.Serialization;
using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.companies
{
    public class Company : Document
    {
        [BsonElement("name")] 
        public string Name { get; set; }
        
        [BsonElement("users")] 
        public IList<PropertyUser> Users { get; set; }
        
        [BsonElement("project_ids")] 
        public IList<ObjectId> ProjectIds { get; set; }
    }
}