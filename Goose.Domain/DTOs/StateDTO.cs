using Goose.Domain.Models.projects;
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
        public StateDTO(State state)
        {
            Id = state._id;
            Name = state.Name;
            Phase = state.Phase;
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
    }
}
