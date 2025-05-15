using GimGim.EventSystem;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Raised when a game action's Prepare phase is completed.
    /// </summary>
    public class GameActionPreparedEvent : EventData {
        public IGameAction Action { get; }
        public GameActionPreparedEvent(object sender, IGameAction action)
            : base(sender) {
            Action = action;
        }
    }

    /// <summary>
    /// Raised when a game action's Perform phase is completed.
    /// </summary>
    public class GameActionPerformedEvent : EventData {
        public IGameAction Action { get; }

        public GameActionPerformedEvent(object sender, IGameAction action) : base(sender) {
            Action = action;
        }
    }
    
    /// <summary>
    /// Raised when a game action is canceled.
    /// </summary>
    public class GameActionCanceledEvent : EventData {
        public IGameAction Action { get; }

        public GameActionCanceledEvent(object sender, IGameAction action) : base(sender) {
            Action = action;
        }
    }

    /// <summary>
    /// Raised when a game action and all its sub-actions are completed.
    /// </summary>
    public class GameActionCompletedEvent : EventData {
        public IGameAction Action { get; }
        public GameActionCompletedEvent(object sender, IGameAction action) : base(sender) {
            Action = action;
        }
    }

    /// <summary>
    /// Raised when the main flow for a game action starts.
    /// </summary>
    public class GameActionFlowStartedEvent : EventData {
        public IGameAction Action { get; }

        public GameActionFlowStartedEvent(object sender, IGameAction action) : base(sender) {
            Action = action;
        }
    }

    /// <summary>
    /// Raised when the main flow for a game action ends.
    /// </summary>
    public class GameActionFlowCompletedEvent : EventData {
        public IGameAction Action { get; }

        public GameActionFlowCompletedEvent(object sender, IGameAction action) : base(sender) {
            Action = action;
        }
    }

    public interface IPostResolutionEvent {
        public bool Repeats { get; }
        public IGameAction Action { get; set; }
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
}