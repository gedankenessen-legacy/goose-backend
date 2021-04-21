using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.Companies;
using Goose.Domain.Models.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Projects
{
    public class Project: Document, IPropertyUsers
    {
        public Project()
        {
            Users = new List<PropertyUser>();
            States = new List<State>();
        }

        public ObjectId CompanyId { get; set; }
        [BsonElement("projectUsers")]
        public IList<PropertyUser> Users { get; set; }
        public ProjectDetail ProjectDetail { get; set; }
        public IList<State> States { get; set; }
    }
}