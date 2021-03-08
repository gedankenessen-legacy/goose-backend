using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Goose.Data.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models
{
    public class Project : Document
    {
        public ProjectUsers[] Users { get; set; }
        public ProjectDetails Details { get; set; }
        public State[] States { get; set; }
    }

    public class ProjectUsers : Document
    {
        [BsonElement("user_id")]
        public string UserID { get; set; }
        [BsonElement("role_ids")]
        public string[] RoleIDs { get; set; }
    }

    public class ProjectDetails
    {
        public string Name { get; set; }
    }

    public class State : Document
    {
        // These are the only valid values for Phase
        const string NegotiationPhase = "Negotiation";
        const string InProcessPhase = "InProcess";
        const string ConclusionPhase = "Conclusion";

        public string Name { get; set; }
        public string Phase { get; set; }
        public bool UserGenerated { get; set; }
    }
}
