using System;
using System.Collections;
using System.Collections.Generic;
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
        public List<GameAction> SubActions { get; } = new();

        protected GameAction() {
            OrderOfPlay = ActionSystem.OrderOfPlayCounter.Next();
            AddPhase(new GameActionPhase(this, OnPrepare));
            AddPhase(new GameActionPhase(this, OnPerform));
        }

        private void AddPhase(GameActionPhase phase) => Phases.Add(phase);
        
        public virtual void Cancel() => IsCanceled = true;

        /// <summary>
        /// Virtual method called during the Prepare phase.
        /// </summary>
        protected virtual void OnPrepare(IContainer game) =>
            NotificationEventSystem.PostEvent(new GameActionPreparedEvent(Sender, this));

        /// <summary>
        /// Virtual method called during the Perform phase.
        /// </summary>
        protected virtual void OnPerform(IContainer game) =>
            NotificationEventSystem.PostEvent(new GameActionPerformedEvent(Sender, this));
    }
}