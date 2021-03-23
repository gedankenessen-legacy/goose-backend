using Goose.Domain.Models;
using Goose.Domain.Models.projects;
using MongoDB.Bson;
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
        public ObjectId Id { get; set; }
        public string Name { get; set; }

        public ProjectDTO()
        {

        }

        public ProjectDTO(Project project)
        {
            Id = project.Id;
            Name = project.ProjectDetail?.Name;
        }
    }
}
