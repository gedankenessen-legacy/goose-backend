#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Goose.Domain.Models.Issues;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.Issues
{
    public class IssueDTO
    {
        public IssueDTO(Issue issue, StateDTO state, ProjectDTO project, UserDTO client, UserDTO author)
        {
            Id = issue.Id;
            IssueDetail = issue.IssueDetail;

            State = state;
            Project = project;
            Client = client;
            Author = author;
        }

        public IssueDTO()
        {
        }

        public ObjectId Id { get; set; }
        public DateTime CreatedAt => Id.CreationTime;

        public StateDTO? State { get; set; }
        [Required] public ProjectDTO Project { get; set; }
        [Required] public UserDTO Client { get; set; }
        [Required] public UserDTO Author { get; set; }
        [Required] public IssueDetail IssueDetail { get; set; }
    }
}