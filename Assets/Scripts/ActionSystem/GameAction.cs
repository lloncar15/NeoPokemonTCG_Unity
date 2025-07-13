using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodiceApp.EventTracking;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.Utility;
using Unity.Profiling.LowLevel.Unsafe;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Abstract class representing a game action. A game action consists of a sequence
    /// of phases (Prepare and Perform) that are executed in order. After each phase,
    /// the action system checks for and processes any reactions to that phase.
    /// </summary>
    public abstract class GameAction : EventData, IGameAction {
        /// <summary>
        /// The priority of the action. Used to determine action resolution order.
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// The order in which the action was added to the system. Used for sorting actions with the same priority.
        /// </summary>
        public int OrderOfPlay { get; set; }
        public bool IsCanceled { get; protected set; }
        public List<GameActionPhase> Phases { get; } = new();

        /// <summary>
        /// Cached event types for this action. This is used to avoid reflection overhead
        /// </summary>
        private readonly GameActionEventTypes _eventTypes;

        protected GameAction() {
            OrderOfPlay = ActionSystem.OrderOfPlayCounter.Next();

            _eventTypes = GameActionTypeRegistry.GetEventTypes(GetType());
            
            AddPhase(new GameActionPhase(this, OnPrepare, GameActionPhaseType.Prepare));
            AddPhase(new GameActionPhase(this, OnPerform, GameActionPhaseType.Perform));
        }

        protected GameAction(object sender) : base(sender) {
            OrderOfPlay = ActionSystem.OrderOfPlayCounter.Next();
            
            _eventTypes = GameActionTypeRegistry.GetEventTypes(GetType());
            
            AddPhase(new GameActionPhase(this, OnPrepare, GameActionPhaseType.Prepare));
            AddPhase(new GameActionPhase(this, OnPerform, GameActionPhaseType.Perform));
        }

        private void AddPhase(GameActionPhase phase) => Phases.Add(phase);
        
        public GameActionPhase GetPhase(GameActionPhaseType phaseType) {
            return Phases.FirstOrDefault(p => p.Type == phaseType);
        }
        
        public virtual void Cancel() => IsCanceled = true;

        /// <summary>
        /// Post the prepared event using cached delegate from TypeRegistry.
        /// </summary>
        protected virtual void OnPrepare(IContainer game) {
            _eventTypes.PostPrepared(Sender, this);
        }

        /// <summary>
        /// Post the performed event using cached delegate from TypeRegistry.
        /// </summary>
        protected virtual void OnPerform(IContainer game) {
            _eventTypes.PostPerformed(Sender, this);
        }

        /// <summary>
        /// Internal methods for ActionSystem to post events related to the flow of the action.
        /// </summary>
        /// <param name="sender"></param>
        #region Internal Event Posting Methods

        internal void PostFlowStarted(object sender) {
            _eventTypes.PostFlowStarted(sender, this);
        }
        
        internal void PostFlowCompleted(object sender) {
            _eventTypes.PostFlowCompleted(sender, this);
        }
        
        internal void PostCompleted(object sender) {
            _eventTypes.PostCompleted(sender, this);
        }
        
        internal void PostCanceled(object sender) {
            _eventTypes.PostCanceled(sender, this);
        }

        #endregion
    }

    public enum GameActionPhaseType {
        Prepare,
        Perform
    }
}