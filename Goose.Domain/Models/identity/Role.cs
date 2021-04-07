using Goose.Data.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.Identity
{
    public class Role : Document
    {
        public string Name { get; set; }

        public const string CompanyRole = "Firma";
        public const string CustomerRole = "Kunde";
        public const string ProjectLeaderRole = "Projektleiter";
        public const string EmployeeRole = "Mitarbeiter";
        public const string ReadonlyEmployeeRole = "Mitarbeiter (Lesend)";
    }
}