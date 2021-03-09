using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Goose.Domain.Models.tickets
{
    public class IssueDetail
    {
        
        [BsonElement("name")] 
        public string Name { get; set; }
        
        [BsonElement("type")] 
        public string Type { get; set; }
        
        [BsonElement("startDate")] 
        public DateTime StartDate { get; set; }
        
        [BsonElement("endDate")] 
        public DateTime EndDate { get; set; }
        
        [BsonElement("expectedTime")] 
        public double ExpectedTime { get; set; }
        
        [BsonElement("progress")] 
        public int Progress { get; set; }
        
        [BsonElement("description")] 
        public string Description { get; set; }
        
        [BsonElement("requirements")] 
        public IList<IssueRequirement> Requirements { get; set; }
        
        [BsonElement("requirementsAccepted")] 
        public bool RequirementsAccepted { get; set; }
        
        [BsonElement("requirementsNeeded")] 
        public bool RequirementsNeeded { get; set; }
        
        [BsonElement("priority")] 
        public int Priority { get; set; }
        
        [BsonElement("finalComment")] 
        public string FinalComment { get; set; }
        
        [BsonElement("visibility")] 
        public bool Visibility { get; set; }
        
        [BsonElement("relevantDocuments")] 
        public IList<string> RelevantDocuments { get; set; }
    }
}