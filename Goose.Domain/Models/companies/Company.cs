using System.Collections.Generic;
using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Companies
{
    public class Company : Document, IPropertyUsers
    {
        public string Name { get; set; }

        [BsonElement("users")]
        public IList<PropertyUser> Users { get; set; }
        public IList<ObjectId> ProjectIds { get; set; }
    }
}