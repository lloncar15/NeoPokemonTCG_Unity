using GimGim.ActionSystem;
using GimGim.ActionSystem.GameActions;
using GimGim.AspectContainer;
using GimGim.EventSystem;
using GimGim.Model;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// Gameplay system that manages the match handling logic like changing turns, etc.
    /// </summary>
    public class MatchSystem : GameplaySystem {
        public override void Awake() {
            Subscribe(new EventSubscription<ChangeTurnAction>(OnChangeTurnPerformed));
        }

        public override void Destroy() {
            UnsubscribeAllAndClear();
        }

        public void ChangeTurn() {
            Match match = Container.GetMatch();
            int nextPlayerIndex = (1 - match.CurrentPlayerIndex);
            ChangeTurn(nextPlayerIndex);
        }

        public void ChangeTurn(int nextPlayerIndex) {
            ChangeTurnAction action = new(nextPlayerIndex);
            Container.PerformGameAction(action);
        }
        
        void OnChangeTurnPerformed(ChangeTurnAction action) {
            Match match = Container.GetMatch();
            match.CurrentPlayerIndex = action.TargetPlayerIndex;
        }
    }
}