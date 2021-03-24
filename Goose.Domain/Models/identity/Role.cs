using Goose.Data.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.identity
{
    public class Role : Document
    {
        public string Name { get; set; }

        public const string CompanyRole = "Company";
        public const string CustomerRole = "Customer";
        public const string ProjectLeaderRole = "ProjectLeader";
        public const string EmployeeRole = "Employee";
        public const string ReadonlyRole = "Readonly";
    }
}