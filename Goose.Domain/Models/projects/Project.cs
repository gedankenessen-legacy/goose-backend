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
        public Project()
        {
            ProjectUsers = new List<PropertyUser>();
            States = new List<State>();
        }

        public ObjectId CompanyId { get; set; }
        public IList<PropertyUser> ProjectUsers { get; set; }
        public ProjectDetail ProjectDetail { get; set; }
        public IList<State> States { get; set; }
    }
}