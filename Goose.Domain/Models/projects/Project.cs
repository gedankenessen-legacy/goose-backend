using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.companies;
using Goose.Domain.Models.identity;
using MongoDB.Bson;

namespace Goose.Domain.Models.projects
{
    public class Project: Document
    {
        public ObjectId CompanyId { get; set; }
        public IList<User> ProjectUsers { get; set; }
        public ProjectDetail ProjectDetail { get; set; }
        public IList<State> States { get; set; }
    }
}