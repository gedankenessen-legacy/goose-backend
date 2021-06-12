using Goose.Domain.DTOs;

namespace Goose.API.Services.issues.IssueStateHandler
{
    public interface IFsmEvent<TFunc>
    {
        public TFunc Func { get; set; }

        public bool IsValidEvent(StateDTO oldState, StateDTO newState);
    }

    public class FsmEvent<TFunc> : IFsmEvent<TFunc>
    {
        public string OldState { get; set; }
        public string NewState { get; set; }
        public TFunc Func { get; set; }

        public bool IsValidEvent(StateDTO oldState, StateDTO newState)
        {
            return OldState + oldState.Name == oldState.Name + OldState && NewState == newState.Name;
        }
    }
}