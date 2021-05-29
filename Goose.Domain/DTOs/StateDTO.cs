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

        public static bool operator <(StateDTO state, StateDTO other)
        {
            if (state.Phase == other.Phase) return false;
            if (state.Phase == State.NegotiationPhase) return true;
            if (other.Phase == State.NegotiationPhase) return false;
            if (state.Phase == State.ProcessingPhase) return true;
            if (other.Phase == State.ProcessingPhase) return false;
            throw new Exception($"could not compare {state.Phase} and {other.Phase}");
        }

        public static bool operator >(StateDTO state, StateDTO other)
        {
            if (state.Phase == other.Phase) return false;
            if (state.Phase == State.NegotiationPhase) return false;
            if (other.Phase == State.NegotiationPhase) return true;
            if (state.Phase == State.ProcessingPhase) return false;
            if (other.Phase == State.ProcessingPhase) return true;
            throw new Exception($"could not compare {state.Phase} and {other.Phase}");
        }

        public static bool operator >=(StateDTO state, StateDTO other)
        {
            if (state.Phase == other.Phase) return true;
            return state > other;
        }

        public static bool operator <=(StateDTO state, StateDTO other)
        {
            if (state.Phase == other.Phase) return true;
            return state < other;
        }
    }
}