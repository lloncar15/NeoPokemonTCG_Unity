using System.Collections.Generic;
using System;

namespace GimGim.StateMachine {
    /// <summary>
    /// Builder for constructing a single CompositeState tree (one phase of the game).
    /// </summary>
    /// <typeparam name="TPhase">Phase type we are constructing</typeparam>
    public class PhaseBuilder<TPhase> where TPhase : CompositeState, new() {
        private readonly TPhase _root;
        private readonly Dictionary<Type, State> _instances = new Dictionary<Type, State>();
        private int _compositeDepth;
        private CompositeState _currentCompositeState;
        
        public PhaseBuilder() {
            _root = new TPhase();
            _currentCompositeState = _root;
        }

        /// <summary>
        /// Define a nested composite state under this phase and go one level deeper in the nesting.
        /// </summary>
        public PhaseBuilder<TPhase> WithComposite<TComposite>() where TComposite : CompositeState, new() {
            Type type = typeof(TComposite);

            if (_instances.ContainsKey(type)) {
                throw new InvalidOperationException($"Composite state of type {type.Name} is already added.");
            }

            CompositeState composite = (CompositeState)Activator.CreateInstance(type);
            _instances[type] = composite;
            _currentCompositeState.AddChildState(composite);
            _currentCompositeState = composite;
            _compositeDepth++;
            return this;
        }
        
        /// <summary>
        /// Signs the ending of the nested composite state and goes one level shallower.
        /// </summary>
        public PhaseBuilder<TPhase> EndComposite() {
            if (_compositeDepth <= 0) {
                throw new InvalidOperationException("EndComposite called without a matching WithComposite.");
            }

            CompositeState parent = _currentCompositeState.Parent as CompositeState ?? _root;
            _currentCompositeState = parent;
            _compositeDepth--;
            return this;
        }

        /// <summary>
        /// Define a state under the current composite state.
        /// </summary>
        public PhaseBuilder<TPhase> State<TState>() where TState : State, new() {
            Type type = typeof(TState);

            if (_instances.ContainsKey(type)) {
                throw new InvalidOperationException($"State of type {type.Name} is already added.");
            }

            State instance = (State)Activator.CreateInstance(type);
            _instances[type] = instance;
            _currentCompositeState.AddChildState(instance);
            return this;
        }
        
        public TPhase Build() {
            return _root;
        }
    }

    /// <summary>
    /// Builder for constructing the StateMachine.
    /// </summary>
    /// <typeparam name="TRoot">Type of the root Composite state</typeparam>
    public class MachineBuilder<TRoot> where TRoot : CompositeState, new() {
        private readonly TRoot _root = new();

        public MachineBuilder<TRoot> Phase(CompositeState phase) {
            if (phase == null) {
                throw new ArgumentNullException(nameof(phase), "Phases cannot be null.");
            }

            _root.AddChildState(phase);
            return this;
        }

        public StateMachine Build() {
            return new StateMachine(_root);
        }
    }

    /*
     Builder layout to show the construction
     //TODO: remove when concrete implementation of turns is made and the builder is used for the first time
     
    public static class Test2 {
        public static void TestBuilder() {
            StartMatchPhase startPhase = new PhaseBuilder<StartMatchPhase>()
                .WithComposite<StartMatchSetupStateInner>()
                        .State<StartingPlayerSelectionState>()
                    .EndComposite()
                .State<StartMatchSetupState>()
                .State<BasicPokemonCheckupState>()
                .State<BasicSelectionState>()
                .State<StartingPlayerSelectionState>()
                .Build();
            
            CompositeState turnPhase = new PhaseBuilder<TurnPhase>()
                .WithComposite<AttackPhase>()
                        .State<BabyAttackState>()
                        .State<TargetChoosingState>()
                        .State<AttackPrerequisiteState>()
                        .State<AttackAlteringState>()
                    .EndComposite()
                .Build();

            CompositeState endPhase = new PhaseBuilder<EndPhase>()
                .Build();
            
            StateMachine machine = new MachineBuilder<BaseMatchState>()
                .Phase(startPhase)
                .Phase(turnPhase)
                .Phase(endPhase)
                .Build();
        }
    }

    public class StartMatchPhase : CompositeState { }
    public class StartMatchSetupState : State { }
    public class StartMatchSetupStateInner : CompositeState { }
    public class BasicPokemonCheckupState : State { }
    public class BasicSelectionState : State { }
    public class StartingPlayerSelectionState : State { }
        
    public class TurnPhase : CompositeState { }
        
    public class AttackPhase : CompositeState { }
    public class BabyAttackState : State { }
    public class TargetChoosingState : State { }
    public class AttackPrerequisiteState : State { }
    public class AttackAlteringState : State { }
        
    public class EndPhase : CompositeState { }
        
    public class BaseMatchState : CompositeState { }
    
    */
}