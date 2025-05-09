namespace GimGim.StateMachine {
    /// <summary>
    /// Base abstract class for all states, supporting hierarchical nesting and guarding transitions.
    /// </summary>
    public abstract class State {
        public State Parent { get; internal set; }
        protected StateMachine StateMachine;

        internal void SetStateMachineContext(StateMachine machine) {
            StateMachine = machine;
        }
        
        /// <summary>
        /// Called when entering the state. Override to implement custom behavior.
        /// </summary>
        public virtual void OnEnter() {}
        /// <summary>
        /// Called when exiting the state. Override to implement custom behavior.
        /// </summary>
        public virtual void OnExit() {}
        /// <summary>
        /// Called every frame while in the state. Override to implement custom behavior.
        /// </summary>
        public virtual void OnUpdate() {}
        
        /// <summary>
        /// Guards the transition to a target state. Override to implement custom behavior.
        /// </summary>
        /// <param name="targetState">State trying to transition to</param>
        public virtual bool CanExit(State targetState) => true;
        
        /// <summary>
        /// Guards the transition to this state from a previous state. Override to implement custom behavior.
        /// </summary>
        /// <param name="previousState">State trying to transition from</param>
        public virtual bool CanEnter(State previousState) => true;
    }
}