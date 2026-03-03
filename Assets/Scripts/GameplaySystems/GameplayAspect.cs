using System;
using System.Collections.Generic;
using GimGim.AspectContainer;
using GimGim.EventSystem;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// A class that is observing and sending events, while being an Aspect.
    /// </summary>
    public class GameplayAspect : Aspect, IObserve {
        /// <summary>
        /// List of subscriptions to the NotificationEventSystem.
        /// </summary>
        private readonly List<IEventSubscription> _subscriptions = new();
        
        /// <summary>
        /// Posts a given event to the NotificationEventSystem.
        /// </summary>
        public void PostEvent<T>(T eventData) where T : EventData {
            NotificationEventSystem.PostEvent(eventData);
        }

        /// <summary>
        /// Posts a given event to the NotificationEventSystem and executes it immediately.
        /// </summary>
        public void PostAndExecuteEvent<T>(T eventData) where T : EventData {
            NotificationEventSystem.PostEventAndExecute(eventData);
        }

        /// <summary>
        /// Add the given subscription into the list of subscriptions and subscribe to the NotificationEventSystem.
        /// </summary>
        public IEventSubscription Subscribe(IEventSubscription subscription) {
            _subscriptions.Add(subscription);
            return NotificationEventSystem.Subscribe(subscription);
        }

        /// <summary>
        /// Removes the given subscription from the list of subscriptions.
        /// </summary>
        public void Unsubscribe(IEventSubscription subscription) {
            _subscriptions.Remove(subscription);
        }

        /// <summary>
        /// Subscribes all the events in the subscriptions list to the NotificationEventSystem.
        /// </summary>
        public void SubscribeAll() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Subscribe(subscription);
            }
        }

        /// <summary>
        /// Unsubscribes all the events in the subscriptions list to the NotificationEventSystem.
        /// </summary>
        public void UnsubscribeAll() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Unsubscribe(subscription);
            }
        }

        /// <summary>
        /// Unsubscribes all the events in the subscriptions list to the NotificationEventSystem and clears the list.
        /// </summary>
        public void UnsubscribeAllAndClear() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Unsubscribe(subscription);
            }
            _subscriptions.Clear();
        }
    }
}