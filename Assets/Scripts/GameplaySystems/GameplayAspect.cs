using System;
using System.Collections.Generic;
using GimGim.AspectContainer;
using GimGim.EventSystem;

namespace GimGim.GameplaySystems {
    public class GameplayAspect : Aspect, IObserve {
        private readonly List<IEventSubscription> _subscriptions = new();
        
        public void PostEvent<T>(T eventData) where T : EventData {
            NotificationEventSystem.PostEvent(eventData);
        }

        public void PostAndExecuteEvent<T>(T eventData) where T : EventData {
            NotificationEventSystem.PostEventAndExecute(eventData);
        }

        public IEventSubscription Subscribe(IEventSubscription subscription) {
            _subscriptions.Add(subscription);
            return NotificationEventSystem.Subscribe(subscription);
        }

        public void Unsubscribe(IEventSubscription subscription) {
            _subscriptions.Remove(subscription);
        }

        public void SubscribeAll() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Subscribe(subscription);
            }
        }

        public void UnsubscribeAll() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Unsubscribe(subscription);
            }
        }

        public void UnsubscribeAllAndClear() {
            foreach (IEventSubscription subscription in _subscriptions) {
                NotificationEventSystem.Unsubscribe(subscription);
            }
            _subscriptions.Clear();
        }
    }
}