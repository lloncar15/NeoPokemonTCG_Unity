using GimGim.EventSystem;

namespace GimGim.StateMachine {
    public abstract class StateMachineEvent : EventData {
        public State From { get; }
        public State To { get; }
        
        public StateMachineEvent(object sender, State from, State to) : base(sender) {
            From = from;
            To = to;
        }
    }
    
    /// <summary>
    /// Event called when a state transition occurs.
    /// </summary>
    public class StateTransitionEvent : StateMachineEvent {
        public StateTransitionEvent(object sender, State from, State to) : base(sender, from, to) {}
    }

    /// <summary>
    /// Event called when a state transition is blocked.
    /// </summary>
    public class StateTransitionBlocked : StateMachineEvent {
        public StateTransitionBlocked(object sender, State from, State to) : base(sender, from, to) {}
    }
}