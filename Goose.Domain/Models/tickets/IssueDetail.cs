using System;
using System.Collections.Generic;

namespace Goose.Domain.Models.tickets
{
    public class IssueDetail
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double ExpectedTime { get; set; }
        public int Progress { get; set; }
        public string Description { get; set; }
        public IList<IssueRequirement> Requirements { get; set; }
        public bool RequirementsAccepted { get; set; }
        public bool RequirementsSummaryCreated { get; set; }
        public bool RequirementsNeeded { get; set; }
        public int Priority { get; set; }
        public bool Visibility { get; set; }
        public IList<string> RelevantDocuments { get; set; }
    }
}