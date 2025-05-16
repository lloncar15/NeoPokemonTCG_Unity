using System;
using System.Linq;
using GimGim.Utility;

namespace GimGim.EventSystem {
    /// <summary>
    /// Interface for event binding objects.
    /// </summary>
    public interface IEventSubscription {
        bool UsedOnce { get; }
        int Priority { get; }
        int TypeHash { get; }
        
        void Invoke(IEvent eventData);
    }
    
    /// <summary>
    /// Represents a subscription to an event in the event system.
    /// </summary>
    /// <typeparam name="T">The type of event this subscription listens to.</typeparam>
    public class EventSubscription<T> : IEventSubscription where T : IEvent {
        private readonly Action<T> _action;
        public bool UsedOnce { get; }
        public int Priority { get; }
        public int TypeHash { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscription{T}"/> class.
        /// </summary>
        /// <param name="action">The action to invoke when the event is dispatched.</param>
        /// <param name="usedOnce">Indicates if the subscription should be removed after the first invocation.</param>
        /// <param name="priority">The priority of the subscription.</param>
        public EventSubscription(Action<T> action, bool usedOnce = false, int priority = 0) {
            _action = action;
            UsedOnce = usedOnce;
            Priority = priority;
            TypeHash = TypeRegistry.GetTypeHashes(typeof(T)).FirstOrDefault();
        }

        /// <summary>
        /// Invokes the subscription's action with the provided event data.
        /// </summary>
        /// <param name="eventData">The event data to pass to the action.</param>
        public void Invoke(IEvent eventData) {
            _action?.Invoke((T)eventData);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current subscription.
        /// </summary>
        /// <param name="obj">The object to compare with the current subscription.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object obj) {
            if (obj is EventSubscription<T> eventBinding) {
                return _action == eventBinding._action;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this subscription based on the hash code of the action multiplied by 17 to reduce collisions.
        /// </summary>
        /// <returns>The hash code for this subscription.</returns>
        public override int GetHashCode() {
            return _action.GetHashCode() * 17;
        }
    }
}