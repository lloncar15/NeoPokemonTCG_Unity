using System.Collections.Generic;
using GimGim.NotificationEventSystem;

namespace GimGim.StateMachine {
    /// <summary>
    /// Hierarchical finite state machine managing nested state transitions which supports guarded transitions.
    /// </summary>
    public class StateMachine {
        private readonly CompositeState _root;
        private State _currentState;
        
        public StateMachine(CompositeState root) {
            _root = root;
            InitializeContext(root);
        }

        private void InitializeContext(CompositeState root) {
            root.SetStateMachineContext(this);
            foreach (State child in root.Children) {
                if (child is CompositeState composite) {
                    InitializeContext(composite);
                }
                else {
                    child.SetStateMachineContext(this);
                }
            }
        }
        
        /// <summary>
        /// Begin the finite state machine, entering the root state.
        /// </summary>
        public void StartStateMachine() {
            EnterRecursive(_root);
        }

        private void EnterRecursive(State state) {
            state.OnEnter();
            if (state is CompositeState composite && composite.GetInitialState() != null) {
                EnterRecursive(composite.GetInitialState());
            }
            else {
                _currentState = state;
            }
        }

        /// <summary>
        /// Called from the Unity game loop to update the current state.
        /// </summary>
        public void Update() {
            _currentState?.OnUpdate();
        }

        /// <summary>
        /// Attempts to transition to a target state. If the transition is guarded, it will not occur.
        /// </summary>
        /// <param name="target">The state we are trying to transition to</param>
        public bool TransitionTo(State target) {
            if (_currentState == null || target == null) {
                return false;
            }
            
            if (!_currentState.CanExit(target) && !target.CanEnter(_currentState)) {
                NotificationEventSystem.NotificationEventSystem.PostEvent(new StateTransitionBlocked(this, _currentState, target));
                return false;
            }
            
            // Perform least common ancestor-based exit and enter
            State lca = FindLeastCommonAncestor(_currentState, target);
            
            for (State state = _currentState; state != lca; state = state.Parent) {
                state.OnExit();
            }
            
            Stack<State> stack = new Stack<State>();
            for (State state = target; state != lca; state = state.Parent) {
                stack.Push(state);
            }
            
            while (stack.Count > 0) {
                State state = stack.Pop();
                state.OnEnter();
            }
            
            State old = _currentState;
            _currentState = target;
            NotificationEventSystem.NotificationEventSystem.PostEvent(new StateTransitionEvent(this, old, _currentState));
            return true;
        }

        /// <summary>
        /// Finds the least common ancestor of two states in the state machine hierarchy.
        /// LCA is the deepest state that is an ancestor of both a and b.
        /// </summary>
        private static State FindLeastCommonAncestor(State a, State b) {
            HashSet<State> ancestors = new HashSet<State>();
            for (State s = a; s != null; s = s.Parent) {
                ancestors.Add(s);
            }
            for (State s = b; s != null; s = s.Parent) {
                if (ancestors.Contains(s)) {
                    return s;
                }
            }

            return null;
        }
    }
}