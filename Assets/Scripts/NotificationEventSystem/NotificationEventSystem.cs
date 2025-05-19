using System.Collections.Generic;
using System.Linq;

namespace GimGim.EventSystem {
    /// <summary>
    /// A singleton-based event system for managing event subscriptions and dispatching events.
    /// </summary>
    public class NotificationEventSystem {
        private static NotificationEventSystem _instance;

        /// <summary>
        /// Gets the singleton instance of the event system.
        /// </summary>
        public static NotificationEventSystem Instance {
            get { return _instance ??= new NotificationEventSystem(); }
        }
        
        private readonly Queue<EventData> _queue = new();
        private readonly Dictionary<int, List<IEventSubscription>> _subscriptions = new();

        /// <summary>
        /// Subscribes to an event with the given subscription.
        /// </summary>
        /// <param name="subscription">The subscription to add.</param>
        /// <returns>The added subscription.</returns>
        public static IEventSubscription Subscribe(IEventSubscription subscription) {
            return Instance.AddToSubscriptions(subscription);
        }
        
        private IEventSubscription AddToSubscriptions(IEventSubscription subscription) {
            int hash = subscription.TypeHash;
            
            if (!_subscriptions.ContainsKey(hash)) {
                _subscriptions[hash] = new List<IEventSubscription>();
            }
            
            var listOfSubs = _subscriptions[hash];
            int index = listOfSubs.BinarySearch(subscription, Comparer<IEventSubscription>.Create((a,b) => b.Priority.CompareTo(a.Priority)));
            if (index < 0) index = ~index;
            listOfSubs.Insert(index, subscription);

            return subscription;
        }

        /// <summary>
        /// Unsubscribes from an event by removing the given subscription.
        /// </summary>
        /// <param name="subscription">The subscription to remove.</param>
        public static void Unsubscribe(IEventSubscription subscription) {
            Instance.RemoveFromSubscriptions(subscription);
        }

        private void RemoveFromSubscriptions(IEventSubscription subscription) {
            int hash = subscription.TypeHash;

            if (_subscriptions.ContainsKey(hash)) {
                if (_subscriptions[hash].Contains(subscription)) {
                    _subscriptions[hash].Remove(subscription);
                }
            }
        }

        /// <summary>
        /// Posts an event to the event queue for later dispatch.
        /// </summary>
        /// <typeparam name="T">The type of event to post.</typeparam>
        /// <param name="eventData">The event data to post.</param>
        public static void PostEvent<T>(T eventData) where T : EventData {
            Instance.AddEventToQueue(eventData);
        }
        
        public static void PostEventInstantly<T>(T eventData) where T : EventData {
            Instance.DispatchEvent(eventData);
        }

        private void AddEventToQueue<T>(T eventData) where T : EventData {
            _queue.Enqueue(eventData);
        }

        public static void Flush() {
            Instance.DispatchEvents();
        }
        
        /// <summary>
        /// Dispatches all events in the queue to their respective subscribers.
        /// </summary>
        public void DispatchEvents() {
            while (_queue.Count > 0) {
                DispatchEvent(_queue.Dequeue());
            }
        }

        /// <summary>
        /// Dispatches a specific event to all relevant subscribers.
        /// </summary>
        /// <typeparam name="T">The type of event to dispatch.</typeparam>
        /// <param name="eventData">The event data to dispatch.</param>
        private void DispatchEvent<T>(T eventData) where T : EventData {
            List<IEventSubscription> subs = _subscriptions
                .Where(k => eventData.TypeHashes.Contains(k.Key))
                .SelectMany(k => k.Value)
                .ToList();
            
            if (subs.Count == 0) return;

            foreach (IEventSubscription sub in subs) {
                sub.Invoke(eventData);
                if (sub.UsedOnce) {
                    Unsubscribe(sub);
                }
            }
        }
    }
}