using System.Collections.Generic;
using MongoDB.Bson;

namespace Goose.Domain.Models.projects
{
    public class ProjectUser
    {
        public ObjectId _id { get; set; }
        public ObjectId UserId { get; set; }
        public IList<ObjectId> RoleIds { get; set; }
    }
}