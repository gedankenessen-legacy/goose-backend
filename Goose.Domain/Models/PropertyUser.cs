using System.Collections.Generic;
using MongoDB.Bson;

namespace Goose.Domain.Models
{
    public class PropertyUser
    {
        public ObjectId UserId { get; set; }
        public IList<ObjectId> RoleIds { get; set; }
    }

    public interface IPropertyUsers
    {
        public IList<PropertyUser> Users { get; set; }
    }
}