using System.Collections.Generic;
using Goose.Data.Models;
using Goose.Domain.Models.projects;
using MongoDB.Bson;

namespace Goose.Domain.Models.companies
{
    public class Company: Document
    {
        public string Name { get; set; }
        public IList<CompanyUser> Users { get; set; }
        public IList<ObjectId> ProjectIds { get; set; }
    }
}