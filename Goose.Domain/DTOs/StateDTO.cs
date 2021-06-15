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

        private static int GetValueOfState(StateDTO state)
        {
            var value = 0;
            switch (state.Phase)
            {
                case State.NegotiationPhase:
                    if (state.Name == State.CheckingState) value += 1;
                    else value += 2;
                    break;
                case State.ProcessingPhase:
                    value += 10;
                    if (state.Name == State.BlockedState || state.Name == State.WaitingState) value += 1;
                    else if (state.Name == State.ReviewState) value += 3;
                    else value += 2;
                    break;
                case State.ConclusionPhase:
                    value += 20;
                    if (state.Name == State.ArchivedState) value += 2;
                    else value += 1;
                    break;
            }

            if (state.Phase == State.ProcessingPhase) value += 10;
            if (state.Phase == State.ConclusionPhase) value += 20;

            return value;
        }

        public static bool operator <(StateDTO state, StateDTO other)
        {
            return GetValueOfState(state) < GetValueOfState(other);
        }

        public static bool operator >(StateDTO state, StateDTO other)
        {
            return GetValueOfState(state) > GetValueOfState(other);
        }

        public static bool operator >=(StateDTO state, StateDTO other)
        {
            return GetValueOfState(state) >= GetValueOfState(other);
        }

        public static bool operator <=(StateDTO state, StateDTO other)
        {
            return GetValueOfState(state) <= GetValueOfState(other);
        }
    }
}