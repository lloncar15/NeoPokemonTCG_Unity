using System;
using GimGim.EventSystem;

namespace GimGim.ActionSystem {
    #region Game Event Interfaces

    /// <summary>
    /// Interface for all prepare events
    /// </summary>
    public interface IGameActionPreparedEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }

    /// <summary>
    /// Interface for all perform events
    /// </summary>
    public interface IGameActionPerformedEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }

    /// <summary>
    /// Interface for all canceled events - allows subscribing to any canceled event
    /// </summary>
    public interface IGameActionCanceledEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }

    /// <summary>
    /// Interface for all completed events
    /// </summary>
    public interface IGameActionCompletedEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }

    /// <summary>
    /// Interface for all flow started events
    /// </summary>
    public interface IGameActionFlowStartedEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }

    /// <summary>
    /// Interface for all flow completed events
    /// </summary>
    public interface IGameActionFlowCompletedEvent : IEvent {
        IGameAction Action { get; }
        Type ActionType { get; }
    }
    
    #endregion
    
    #region Core Generic Events
    /// <summary>
    /// Base class for all game action events that includes the action type information.
    /// </summary>
    public abstract class GameActionEvent<TAction> : EventData where TAction : IGameAction {
        public TAction Action { get; }
        public Type ActionType { get; }

        protected GameActionEvent(object sender, TAction action) : base(sender) {
            Action = action;
            ActionType = action.GetType();
        }
    }
    
    /// <summary>
    /// Generic prepare event that can be used with any GameAction type
    /// </summary>
    public class GameActionPrepared<TAction> : GameActionEvent<TAction>, IGameActionPreparedEvent where TAction : IGameAction {
        IGameAction IGameActionPreparedEvent.Action => Action;
        public GameActionPrepared(object sender, TAction action) : base(sender, action) { }
    }
    
    /// <summary>
    /// Generic perform event that can be used with any GameAction type
    /// </summary>
    public class GameActionPerformed<TAction> : GameActionEvent<TAction>, IGameActionPerformedEvent where TAction : IGameAction {
        IGameAction IGameActionPerformedEvent.Action => Action;
        public GameActionPerformed(object sender, TAction action) : base(sender, action) { }
    }
    
    /// <summary>
    /// Generic canceled event that can be used with any GameAction type
    /// </summary>
    public class GameActionCanceled<TAction> : GameActionEvent<TAction>, IGameActionCanceledEvent where TAction : IGameAction {
        IGameAction IGameActionCanceledEvent.Action => Action;
        public GameActionCanceled(object sender, TAction action) : base(sender, action) { }
    }
    
    /// <summary>
    /// Generic completed event that can be used with any GameAction type
    /// </summary
    public class GameActionCompleted<TAction> : GameActionEvent<TAction>, IGameActionCompletedEvent where TAction : IGameAction {
        IGameAction IGameActionCompletedEvent.Action => Action;
        public GameActionCompleted(object sender, TAction action) : base(sender, action) { }
    }
    
    /// <summary>
    /// Generic flow started event that can be used with any GameAction type
    /// </summary
    public class GameActionFlowStarted<TAction> : GameActionEvent<TAction>, IGameActionFlowStartedEvent where TAction : IGameAction {
        IGameAction IGameActionFlowStartedEvent.Action => Action;
        public GameActionFlowStarted(object sender, TAction action) : base(sender, action) { }
    }
    
    /// <summary>
    /// Generic flow completed event that can be used with any GameAction type
    /// </summary
    public class GameActionFlowCompleted<TAction> : GameActionEvent<TAction>, IGameActionFlowCompletedEvent where TAction : IGameAction {
        IGameAction IGameActionFlowCompletedEvent.Action => Action;
        public GameActionFlowCompleted(object sender, TAction action) : base(sender, action) { }
    }
    #endregion

    #region Post-Resolution Events

    public interface IPostResolutionEvent {
        public bool Repeats { get; }
    }
    
    /// <summary>
    /// Base class for post-resolution events. These events are raised after the main action and its
    /// reactions are completed. They can be used to trigger additional PostResolution flow passes.
    /// </summary>
    public abstract class PostResolutionEvent : EventData, IPostResolutionEvent {
        public bool Repeats { get; }
        public IGameAction Action { get; set; }
        
        protected PostResolutionEvent(object sender, bool repeats) : base(sender) {
            Repeats = repeats;
        }
    }

    #endregion
}