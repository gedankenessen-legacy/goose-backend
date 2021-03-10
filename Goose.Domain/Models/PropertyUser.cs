using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models
{
    public class PropertyUser
    {
        public ObjectId _id { get; set; }
        public ObjectId UserId { get; set; }
        public IList<ObjectId> RoleIds { get; set; }
    }
}