﻿#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Goose.Domain.Models.tickets;
using MongoDB.Bson;

namespace Goose.Domain.DTOs.issues
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

        public Issue ToIssue()
        {
            return new Issue
            {
                Id = Id,
                StateId = State.Id,
                ProjectId = Project.Id,
                ClientId = Client.Id,
                AuthorId = Author.Id,
                IssueDetail = IssueDetail,
                ConversationItems = new List<IssueConversation>(),
                TimeSheets = new List<TimeSheet>(),
                AssignedUserIds = new List<ObjectId>(),
                ParentIssueId = null,
                PredecessorIssueIds = new List<ObjectId>(),
                SuccessorIssueIds = new List<ObjectId>()
            };
        }


        public ObjectId Id { get; set; }
        public DateTime CreatedAt => Id.CreationTime;

        [Required] public StateDTO State { get; set; }
        [Required] public ProjectDTO Project { get; set; }
        [Required] public UserDTO Client { get; set; }
        [Required] public UserDTO Author { get; set; }
        [Required] public IssueDetail IssueDetail { get; set; }
    }
}