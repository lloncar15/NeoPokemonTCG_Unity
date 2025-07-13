using System;
using System.Reflection;
using GimGim.EventSystem;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Holds references to all event types and cached delegates for a specific GameAction to avoid reflection at runtime
    /// </summary>
    public class GameActionEventTypes {
        public Type ActionType { get; }
        public Type PreparedEventType { get; }
        public Type PerformedEventType { get; }
        public Type CanceledEventType { get; }
        public Type CompletedEventType { get; }
        public Type FlowStartedEventType { get; }
        public Type FlowCompletedEventType { get; }

        public Action<object, IGameAction> PostPrepared { get; }
        public Action<object, IGameAction> PostPerformed { get; }
        public Action<object, IGameAction> PostCanceled { get; }
        public Action<object, IGameAction> PostCompleted { get; }
        public Action<object, IGameAction> PostFlowStarted { get; }
        public Action<object, IGameAction> PostFlowCompleted { get; }

        /// <summary>
        /// Creates a new instance of GameActionEventTypes for a specific action type. Also creates the event types
        /// and cached delegates for posting events.
        /// </summary>
        /// <param name="actionType">GameAction type to create the generic types and delegates for</param>
        /// <exception cref="ArgumentException">Thrown if actionType doesn't implement IGameAction</exception>
        public GameActionEventTypes(Type actionType) {
            if (!typeof(IGameAction).IsAssignableFrom(actionType)) {
                throw new ArgumentException($"Type {actionType} must implement IGameAction");
            }

            ActionType = actionType;

            PreparedEventType = typeof(GameActionPrepared<>).MakeGenericType(actionType);
            PerformedEventType = typeof(GameActionPerformed<>).MakeGenericType(actionType);
            CanceledEventType = typeof(GameActionCanceled<>).MakeGenericType(actionType);
            CompletedEventType = typeof(GameActionCompleted<>).MakeGenericType(actionType);
            FlowStartedEventType = typeof(GameActionFlowStarted<>).MakeGenericType(actionType);
            FlowCompletedEventType = typeof(GameActionFlowCompleted<>).MakeGenericType(actionType);

            PostPrepared = CreatePostDelegate(PreparedEventType);
            PostPerformed = CreatePostDelegate(PerformedEventType);
            PostCanceled = CreatePostDelegate(CanceledEventType);
            PostCompleted = CreatePostDelegate(CompletedEventType);
            PostFlowStarted = CreatePostDelegate(FlowStartedEventType);
            PostFlowCompleted = CreatePostDelegate(FlowCompletedEventType);
        }

        /// <summary>
        /// Creates a delegate for posting events of the specified type.
        /// </summary>
        private static Action<object, IGameAction> CreatePostDelegate(Type eventType) {
            MethodInfo postMethod = typeof(NotificationEventSystem)
                .GetMethod(nameof(NotificationEventSystem.PostEventAndExecute))
                ?.MakeGenericMethod(eventType);

            if (postMethod is null) {
                throw new InvalidOperationException($"Could not find PostEventAndExecute method for {eventType}");
            }

            return (sender, action) => {
                object evt = Activator.CreateInstance(eventType, sender, action);
                postMethod.Invoke(null, new[] { evt });
            };
        }
    }
}