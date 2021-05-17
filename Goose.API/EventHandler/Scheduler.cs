using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Goose.API.EventHandler
{
    /// <summary>
    /// Scheduler is a facade for the EventExecutor class
    /// </summary>
    public static class Scheduler
    {
        public static async Task AddEvent(IEvent @event)
        {
            var executor = new EventExecutor(@event);
            await executor.StartEvent();
        }

        public static async Task<EventCanceller> AddCancellableEvent(IEvent @event)
        {
            var executor = new EventExecutor(@event);
            var eventCanceller = executor.GetEventCanceller();
            await executor.StartEvent();
            return eventCanceller;
        }
    }

    /// <summary>
    /// The EventExecutor guides an event through it's lifecycle and actually executes it.
    /// It can also be used to configure how the event will be executed, for example whether the event can be cancelled or not.
    /// </summary>
    public sealed class EventExecutor
    {
        private readonly IEvent _event;
        private EventState _eventState;
        private readonly object _eventStateLock = new object();
        private readonly Lazy<EventCanceller> _eventCanceller;

        internal EventExecutor(IEvent @event)
        {
            _event = @event;
            _eventState = EventState.Initialized;
            _eventCanceller = new Lazy<EventCanceller>(() => new EventCanceller(this));
        }

        /// <summary>
        /// Tries to get an EventCanceller for the event.
        /// If the event is already scheduled or finished, an exception is thrown.
        /// </summary>
        /// <returns>An EventCanceller for the event</returns>
        public EventCanceller GetEventCanceller()
        {
            AssertNotStarted();
            return _eventCanceller.Value;
        }

        private void AssertNotStarted()
        {
            var started = (_eventState == EventState.Initialized || _eventState == EventState.Started);
            if (!started)
            {
                throw new InvalidEventStateException();
            }
        }

        private void AssertAndSetState(EventState expected, EventState @new)
        {
            lock (_eventStateLock)
            {
                if (_eventState != expected)
                {
                    throw new InvalidEventStateException($"Cannot transition event from state {expected} into {@new}");
                }

                _eventState = @new;
            }
        }

        internal async Task StartEvent()
        {
            AssertAndSetState(EventState.Initialized, EventState.Started);
            try
            {
                await _event.OnStarted(this);
            }
            finally
            {
                // Schedule the event regardless whether OnStarted threw an Exception,
                // but don't Schedule if already cancelled.
                var doSchedule = false;

                lock (_eventStateLock)
                {
                    if (_eventState == EventState.Started)
                    {
                        _eventState = EventState.Scheduled;
                        doSchedule = true;
                    }
                }

                if (doSchedule)
                {
                    // NO AWAIT. don't wait until the event is completed or cancelled.
                    var _ = ScheduleEvent();
                }
            }
        }

        private async Task ScheduleEvent()
        {
            try
            {
                await WaitForTime();
                // Stop event from being cancelled any more
                AssertAndSetState(EventState.Scheduled, EventState.Executed);
                await _event.OnTimeReached();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Event was cancelled");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Event was cancelled");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in Scheduled Event: {e.Message}");
            }
        }

        private async Task WaitForTime()
        {
            var delay = _event.Time - DateTime.Now;

            if (delay.Ticks <= 0)
            {
                // Event-Time is in the past, no need to delay
                return;
            }

            if (_eventCanceller.IsValueCreated)
            {
                // event could be cancelled while waiting
                var cancelToken = _eventCanceller.Value.Token;
                await Task.Delay(delay, cancelToken);
            }
            else
            {
                // event cannot be canceled
                await Task.Delay(delay);
            }
        }

        internal async Task<bool> Cancel()
        {
            lock (_eventStateLock)
            {
                // Cannot cancel if already canceled or executed
                var alreadyFinshed = (_eventState == EventState.Cancelled || _eventState == EventState.Executed);
                if (alreadyFinshed)
                {
                    return false;
                }

                _eventState = EventState.Cancelled;
            }
            await _event.OnCancelled();
            return true;
        }

        enum EventState
        {
            Initialized,
            Started,     // When calling Event.OnStarted
            Scheduled,   // When waiting
            Executed,
            Cancelled,
        }
    }

    /// <summary>
    /// An EventCanceller allows an Event to be cancelled.
    /// </summary>
    public sealed class EventCanceller
    {
        private readonly EventExecutor _executor;
        private readonly CancellationTokenSource _tokenSource;

        internal EventCanceller(EventExecutor executor)
        {
            _executor = executor;
            _tokenSource = new CancellationTokenSource();
        }

        internal CancellationToken Token => _tokenSource.Token;

        /// <summary>
        /// Cancels the event. Throws an exception if the OnCancelled Event handler of the event did.
        /// </summary>
        /// <returns>True, if the event was canceled. False, if the event was already cancelled or executed.</returns>
        public async Task<bool> Cancel()
        {
            var result = await _executor.Cancel();
            // Cancel Delay Task
            _tokenSource.Cancel();
            return result;
        }
    }

    /// <summary>
    /// InvalidEventStateException are thrown by the EventExecutor and signal that the operation cannot be done
    /// at this state in the event lifecycle.
    /// </summary>
    public class InvalidEventStateException : Exception
    {
        public InvalidEventStateException()
        {
        }

        public InvalidEventStateException(string? message) : base(message)
        {
        }

        public InvalidEventStateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidEventStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
