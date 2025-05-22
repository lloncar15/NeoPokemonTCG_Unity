using System.Collections.Generic;
using NUnit.Framework;

namespace GimGim.EventSystem {
    /// <summary>
    /// Interface for objects that can subscribe to and post events.
    /// </summary>
    public interface IObserve {
        /// <summary>
        /// Posts the event to the event system.
        /// </summary>
        public void PostEvent<T>(T eventData) where T : EventData;
        
        /// <summary>
        /// Posts the event to the event system and executes it immediately.
        /// </summary>
        public void PostAndExecuteEvent<T> (T eventData) where T : EventData;
        
        /// <summary>
        /// Subscribes to an event with the specified subscription.
        /// </summary>
        public IEventSubscription Subscribe(IEventSubscription subscription);
        
        /// <summary>
        /// Unsubscribes from the event system for the specific subscription.
        /// </summary>
        public void Unsubscribe(IEventSubscription subscription);

        /// <summary>
        /// Subscribes to all events from a subscription container.
        /// </summary>
        public void SubscribeAll();
        
        /// <summary>
        /// Unsubscribes from all events from a subscription container.
        /// </summary>
        public void UnsubscribeAll();
        
        /// <summary>
        /// Unsubscribes from all events and clears subscription container.
        /// </summary>
        public void UnsubscribeAllAndClear();
    }
}