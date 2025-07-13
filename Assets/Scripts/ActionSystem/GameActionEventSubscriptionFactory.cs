using System;
using GimGim.EventSystem;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Helper class to create subscriptions for game action events with specific action types or generic game action events.
    /// </summary>
    public static class GameActionEventSubscriptionFactory {
        #region Type-Specific Subscriptions
        
        /// <summary>
        /// Subscribe to prepare events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToPrepare<TAction>(
            Action<GameActionPrepared<TAction>> action,
            bool usedOnce = false,
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionPrepared<TAction>>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to perform events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToPerform<TAction>(
            Action<GameActionPerformed<TAction>> action, 
            bool usedOnce = false, 
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionPerformed<TAction>>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to flow started events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToFlowStarted<TAction>(
            Action<GameActionFlowStarted<TAction>> action, 
            bool usedOnce = false, 
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionFlowStarted<TAction>>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to flow completed events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToFlowCompleted<TAction>(
            Action<GameActionFlowCompleted<TAction>> action, 
            bool usedOnce = false, 
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionFlowCompleted<TAction>>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to canceled events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToCanceled<TAction>(
            Action<GameActionCanceled<TAction>> action, 
            bool usedOnce = false, 
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionCanceled<TAction>>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to completed events for a specific action type
        /// </summary>
        public static IEventSubscription SubscribeToCompleted<TAction>(
            Action<GameActionCompleted<TAction>> action, 
            bool usedOnce = false, 
            int priority = 0) where TAction : IGameAction {
            
            return new EventSubscription<GameActionCompleted<TAction>>(action, usedOnce, priority);
        }
        
        #endregion

        #region Generic Subscriptions

        /// <summary>
        /// Subscribe to any prepare event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyPrepare(
            Action<IGameActionPreparedEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            return new EventSubscription<IGameActionPreparedEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any perform event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyPerform(
            Action<IGameActionPerformedEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionPerformedEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any canceled event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyCanceled(
            Action<IGameActionCanceledEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionCanceledEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any completed event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyCompleted(
            Action<IGameActionCompletedEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionCompletedEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any flow started event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyFlowStarted(
            Action<IGameActionFlowStartedEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionFlowStartedEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any flow completed event regardless of action type
        /// </summary>
        public static IEventSubscription SubscribeToAnyFlowCompleted(
            Action<IGameActionFlowCompletedEvent> action,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionFlowCompletedEvent>(action, usedOnce, priority);
        }
        
        /// <summary>
        /// Subscribe to any prepare event but only execute the action if the filter condition is met.
        /// </summary>
        public static IEventSubscription SubscribeToAnyPrepareFiltered(
            Action<IGameActionPreparedEvent> action,
            Predicate<IGameAction> filter,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionPreparedEvent>(
                e => {
                    if (filter == null || filter(e.Action)) {
                        action(e);
                    }
                },
                usedOnce,
                priority
            );
        }
        
        /// <summary>
        /// Subscribe to any perform event but only execute the action if the filter condition is met.
        /// </summary>
        public static IEventSubscription SubscribeToAnyPerformFiltered(
            Action<IGameActionPerformedEvent> action,
            Predicate<IGameAction> filter,
            bool usedOnce = false,
            int priority = 0) {
            
            return new EventSubscription<IGameActionPerformedEvent>(
                e => {
                    if (filter == null || filter(e.Action)) {
                        action(e);
                    }
                },
                usedOnce,
                priority
            );
        }

        #endregion
    }
}