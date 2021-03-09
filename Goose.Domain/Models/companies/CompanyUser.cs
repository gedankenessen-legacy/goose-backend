using System.Collections.Generic;
using Goose.Domain.Models.identity;
using MongoDB.Bson;

namespace Goose.Domain.Models.companies
{
    public class CompanyUser
    {
        public int _id { get; set; }
        public ObjectId UserId { get; set; }
        public IList<ObjectId> RoleIds { get; set; }
    }
}