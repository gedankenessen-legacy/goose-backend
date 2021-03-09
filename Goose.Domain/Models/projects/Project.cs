using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.companies;
using Goose.Domain.Models.identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.projects
{
    public class Project: Document
    {
        [BsonElement("company_id")] 
        public ObjectId CompanyId { get; set; }
        
        [BsonElement("users")] 
        public IList<PropertyUser> ProjectUsers { get; set; }
        
        [BsonElement("projectDetail")] 
        public ProjectDetail ProjectDetail { get; set; }
        
        [BsonElement("states")] 
        public IList<State> States { get; set; }
    }
}