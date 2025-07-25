using System;
using System.Collections;
using GimGim.AspectContainer;
using GimGim.Utility.Logger;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Part of the GameAction class, representing a phase of the action. It contains the action's handler for the phase
    /// and a viewer function which is used for showing the action in the UI using coroutines. If the viewer is null,
    /// the handler is called directly without waiting for the viewer to finish (used for test cases and AI decisions).
    /// </summary>
    public class GameActionPhase {
        public readonly GameActionPhaseType Type;
        public readonly GameAction Owner;
        private readonly Action<IContainer> _handler;
        public Func<IContainer, GameAction, IEnumerator> Viewer;

        public GameActionPhase(GameAction owner, Action<IContainer> handler, GameActionPhaseType type) {
            Owner = owner;
            _handler = handler;
            Type = type;
        }

        /// <summary>
        /// Coroutine that executes the phase. If Viewer is set, it runs the viewer coroutine and triggers the handler
        /// at the appropriate time; otherwise, it calls the handler immediately.
        /// </summary>
        public IEnumerator Flow(IContainer game) {
            if (Viewer is not null) {
                IEnumerator viwerControlledFlow = ViewerControllerFlow(game);
                while (viwerControlledFlow.MoveNext()) yield return null;
            }
            else {
                _handler(game);
            }
        }

        /// <summary>
        /// Internal coroutine used by Flow to manage the viewer and ensure the handler is called either when
        /// the viewer signals or after the viewer completes.
        /// </summary>
        private IEnumerator ViewerControllerFlow(IContainer game) {
            bool hasTriggeredHandler = false;
            
            IEnumerator sequence = Viewer(game, Owner);
            while (sequence.MoveNext()) {
                if (sequence.Current is bool and true) {
                    hasTriggeredHandler = true;
                    _handler(game);
                    break;
                }

                yield return null;
            }

            if (!hasTriggeredHandler) _handler(game);
        }
    }
}