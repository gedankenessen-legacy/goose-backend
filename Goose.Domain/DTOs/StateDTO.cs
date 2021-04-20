using Goose.Domain.Models.Projects;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Goose.Domain.DTOs
{
    public class StateDTO
    {
        public StateDTO()
        {
        }

        public StateDTO(State state)
        {
            Id = state.Id;
            Name = state.Name;
            Phase = state.Phase;
            UserGenerated = state.UserGenerated;
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
        public bool UserGenerated { get; set; }
    }
}