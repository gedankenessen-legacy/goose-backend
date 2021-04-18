#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Goose.Domain.Models.Issues.Issue;

namespace Goose.Domain.Models.Issues
{
    public class IssueDetail
    {
        [Required] 
        public string Name { get; set; }

        [RegularExpression("(" + TypeBug + "|" + TypeFeature + ")")]
        [Required]
        public string Type { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? ExpectedTime { get; set; }
        [Required] public int Progress { get; set; }
        public string? Description { get; set; }
        public IList<IssueRequirement>? Requirements { get; set; }
        [Required] public bool RequirementsAccepted { get; set; }
        [Required] public bool RequirementsSummaryCreated { get; set; }
        [Required] public bool RequirementsNeeded { get; set; }
        [Required] public int Priority { get; set; }
        [Required] public bool Visibility { get; set; }
        public IList<string>? RelevantDocuments { get; set; }
    }
}