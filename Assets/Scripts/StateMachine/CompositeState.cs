using System.Collections.Generic;

namespace GimGim.StateMachine {
    /// <summary>
    /// Composite state containing nested substates with an initial child.
    /// </summary>
    public class CompositeState : State {
        private readonly List<State> _children = new List<State>();
        private State _initialState;
        
        public IEnumerable<State> Children => _children;

        public void AddChildState(State child) {
            if (_children.Count <= 0) {
                _initialState = child;
            }

            child.Parent = this;
            _children.Add(child);
        }
        
        internal State GetInitialState() => _initialState;
    }
}