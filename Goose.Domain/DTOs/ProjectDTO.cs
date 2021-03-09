using Goose.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Goose.Domain.DTOs
{
    public class ProjectDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }

        public ProjectDTO()
        {

        }

        public ProjectDTO(Project project)
        {
            Id = project.Id;
            Name = project.Details.Name;
            CompanyId = project.CompanyId;
        }
    }
}
