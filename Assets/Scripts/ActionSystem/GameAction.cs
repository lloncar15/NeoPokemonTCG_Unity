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
    public abstract class GameAction : IGameAction {
        /// <summary>
        /// The priority of the action. Used to determine action resolution order.
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// The order in which the action was added to the system. Used for sorting actions with the same priority.
        /// </summary>
        public int OrderOfPlay { get; set; }
        public object Sender { get; set; }
        public bool IsCanceled { get; protected set; }
        public List<GameActionPhase> Phases { get; } = new();

        protected GameAction() {
            OrderOfPlay = ActionSystem.OrderOfPlayCounter.Next();
            AddPhase(new GameActionPhase(this, OnPrepare, GameActionPhaseType.Prepare));
            AddPhase(new GameActionPhase(this, OnPerform, GameActionPhaseType.Perform));
        }

        private void AddPhase(GameActionPhase phase) => Phases.Add(phase);
        
        public GameActionPhase GetPhase(GameActionPhaseType phaseType) {
            switch (phaseType) {
                case GameActionPhaseType.Prepare:
                    return Phases.FirstOrDefault(p => p.Type == GameActionPhaseType.Prepare);
                case GameActionPhaseType.Perform:
                    return Phases.FirstOrDefault(p => p.Type == GameActionPhaseType.Perform);
                default:
                    return null;
            }
        }
        
        public virtual void Cancel() => IsCanceled = true;

        /// <summary>
        /// Virtual method called during the Prepare phase.
        /// </summary>
        protected virtual void OnPrepare(IContainer game) =>
            NotificationEventSystem.PostEventAndExecute(new GameActionPreparedEvent(Sender, this));

        /// <summary>
        /// Virtual method called during the Perform phase.
        /// </summary>
        protected virtual void OnPerform(IContainer game) =>
            NotificationEventSystem.PostEventAndExecute(new GameActionPerformedEvent(Sender, this));
    }

    public enum GameActionPhaseType {
        Prepare,
        Perform
    }
}