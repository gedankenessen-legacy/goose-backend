using System.Collections.Generic;
using System.Text.Json.Serialization;
using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.companies
{
    public class Company : Document
    {
        public string Name { get; set; }
        public IList<PropertyUser> Users { get; set; }
        public IList<ObjectId> ProjectIds { get; set; }
    }
}