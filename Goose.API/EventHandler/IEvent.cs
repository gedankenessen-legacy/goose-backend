using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goose.API.EventHandler
{
    public interface IEvent
    {
        /// <summary>
        /// The time when the event is supposed to be executed.
        /// </summary>
        DateTime Time { get; }

        /// <summary>
        /// Called when the event is about to be started.
        /// This allows the event to configure it's own executor, if necessary.
        /// </summary>
        /// <param name="executor">the eventExecutor that is about to execute the event</param>
        /// <returns>
        /// A Task, to allow the method to be async. If the Task failed, the Event will still be scheduled.
        /// </returns>
        Task OnStarted(EventExecutor executor);

        /// <summary>
        /// This method will be called by the scheduler once the Time is reached.
        /// </summary>
        /// <returns>
        /// A Task, to allow the method to be async. If the Task failed, an error will be logged.
        /// </returns>
        Task OnTimeReached();

        /// <summary>
        /// This method will be called by the scheduler once the Task is cancelled.
        /// It is at most called once, even if cancelling is requested more than once.
        /// </summary>
        /// <returns>
        /// A Task, to allow the method to be async. If the Task failed, the error will be propagated to
        /// the code that called Cancel() on the EventCanceller.
        /// </returns>
        Task OnCancelled();
    }
}
