using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Goose.Domain.Models.projects;

namespace Goose.Domain.DTOs
{
    public class StateDTO
    {
        public StateDTO(State state)
        {
            Id = state._id.ToString();
            Name = state.Name;
            Phase = state.Phase;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Phase { get; set; }
    }
}