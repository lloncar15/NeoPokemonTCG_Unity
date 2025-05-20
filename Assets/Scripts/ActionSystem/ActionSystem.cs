using System.Collections;
using System.Collections.Generic;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.Utility.Counter;
using GimGim.Utility.Logger;
using GimGim.Utility;

namespace GimGim.ActionSystem {
    /// <summary>
    /// The main system that manages the execution and sequencing of game actions, including their phases, subphases,
    /// reactions and post-resolution events. Any change to the model should be done through this class, and any reaction
    /// to the game model changing should be done through the event system.
    /// </summary>
    public class ActionSystem : Aspect {
        private GameAction _rootAction;
        private IEnumerator _rootFlow;
        private List<GameAction> _reactionsToResolve;
        public GameLogger Logger = GameLogger.Create<ActionSystem>(ColorPalette.Blood);
        
        public static readonly ManualCounter OrderOfPlayCounter = new();
        public bool IsActive => _rootFlow != null;
        
        private readonly IActionSystemSorter _sorter = new ActionSystemSorterFiFo();
        
        /// <summary>
        /// List of post-resolution events that will be called once the action and its reactions are completed.
        /// This should be used by systems that perform their actions once all actions in a sequence are completed.
        /// For example, the system handling passive abilities would use this to clean up its state.
        /// </summary>
        private readonly List<IPostResolutionEvent> _postResolutionEvents = new();

        /// <summary>
        /// Called by the Unity game loop wrapper object to update the action system.
        /// </summary>
        public void Update() {
            if (_rootFlow is null) return;

            if (_rootFlow.MoveNext()) return;
            
            NotificationEventSystem.PostEventAndFlush(new GameActionCompletedEvent(this, _rootAction));
            _rootAction = null;
            _rootFlow = null;
            _reactionsToResolve = null;
        }
        
        /// <summary>
        /// Starts the action system with the given action.
        /// </summary>
        /// <param name="action"></param>
        public void PerformGameAction(GameAction action) {
            if (IsActive) return;
            _rootAction = action;
            
            _rootFlow = GameActionFlow(action);
        }

        /// <summary>
        /// Adds a reaction to the action system. The reaction will be executed in the next react phase.
        /// </summary>
        public void AddReaction(GameAction reaction) {
            _reactionsToResolve?.Add(reaction);
        }

        /// <summary>
        /// The main flow of the action system, this is where the action is executed and checking if there are any
        /// reactions to the game action. After gathering reactions, the action system executes them.
        /// The final step is to call all the post-resolution events that are registered in the action system.
        /// </summary>
        private IEnumerator GameActionFlow(GameAction action) {
            NotificationEventSystem.PostEventAndFlush(new GameActionFlowStartedEvent(this, _rootAction));

            foreach (GameActionPhase phase in action.Phases) {
                IEnumerator actionFlow = GameActionPhaseFlow(phase);
                while (actionFlow.MoveNext()) yield return null;
            }

            if (_rootAction == action) {
                foreach (IPostResolutionEvent postResolutionEvent in _postResolutionEvents) {
                    IEnumerator flow = PostActionResolutionFlow(postResolutionEvent);
                    while (flow.MoveNext()) yield return null;
                }
            }
            
            NotificationEventSystem.PostEventAndFlush(new GameActionFlowCompletedEvent(this, _rootAction));
        }

        /// <summary>
        /// The flow of a game action phase. All game actions have a Prepare and Perform phases, and the system
        /// can react accordingly for each phase of the game action.
        /// </summary>
        private IEnumerator GameActionPhaseFlow(GameActionPhase phase) {
            if (phase.Owner.IsCanceled) yield break;
            
            List<GameAction> reactions = _reactionsToResolve = new List<GameAction>();
            IEnumerator actionFlow = phase.Flow(Container);
            while (actionFlow.MoveNext()) yield return null;
            
            IEnumerator reactionsFlow = ReactionsFlow(reactions);
            while (reactionsFlow.MoveNext()) yield return null;
        }

        /// <summary>
        /// The flow of the reaction game actions that are triggered by the game action. Sorts the list of reactions
        /// using the provided sorter and executes them in order.
        /// </summary>
        /// <param name="reactions"></param>
        /// <returns></returns>
        private IEnumerator ReactionsFlow(List<GameAction> reactions) {
            reactions.Sort(_sorter);
            foreach (GameAction reaction in reactions) {
                IEnumerator actionFlow = GameActionFlow(reaction);
                while (actionFlow.MoveNext()) yield return null;
            }
        }
        
        /// <summary>
        /// The final phase of the game action flow where all post-resolution events are called. The calls can be repeated
        /// in the case if any post-resolution event had reactions, so that the post-resolution events can be called.
        /// </summary>
        private IEnumerator PostActionResolutionFlow(IPostResolutionEvent postResolutionEvent) {
            List<GameAction> reactions;
            do {
                reactions = _reactionsToResolve = new List<GameAction>();
                postResolutionEvent.Action = _rootAction;
                NotificationEventSystem.PostEventAndFlush(postResolutionEvent as PostResolutionEvent);

                IEnumerator reactionsFlow = ReactionsFlow(reactions);
                while (reactionsFlow.MoveNext()) yield return null;
            } while (postResolutionEvent.Repeats && reactions.Count > 0);
        }

        /// <summary>
        /// Registers a post-resolution event which is called once the action and its reactions are completed.
        /// </summary>
        /// <param name="postResolutionEvent"></param>
        public void RegisterPostResolutionEvent(IPostResolutionEvent postResolutionEvent) {
            _postResolutionEvents.Add(postResolutionEvent);
        }
    }
}