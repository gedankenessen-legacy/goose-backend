using Goose.Data.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Goose.Domain.Models.Identity
{
    public class Role : Document
    {
        public string Name { get; set; }

        public static readonly Role CompanyRole = new() { Name = "Firma", Id = new ObjectId("604a3420db17824bca29698f") };
        public static readonly Role CustomerRole = new() { Name = "Kunde", Id = new ObjectId("605cc95dd37ccd8527c2ead7") };
        public static readonly Role ProjectLeaderRole = new() { Name = "Projektleiter", Id = new ObjectId("60709abc53608b0ba47360ff") };
        public static readonly Role EmployeeRole = new() { Name = "Mitarbeiter", Id = new ObjectId("605cc555e11e3fa9088d4dd4") };
        public static readonly Role ReadonlyEmployeeRole = new() { Name = "Mitarbeiter (Lesend)", Id = new ObjectId("607aedecbba3b233b8582ae7") };
    }
}