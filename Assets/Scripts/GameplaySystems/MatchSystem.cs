using GimGim.ActionSystem;
using GimGim.EventSystem;
using GimGim.Model;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// Gameplay system that will take care of all match handling logic like changing turns, etc.
    /// </summary>
    public class MatchSystem : GameplaySystem {
        public void ChangeTurn() {
            Match match = Container.GetMatch();
            int nextIndex = (1 - match.CurrentPlayerIndex);
            ChangeTurn(nextIndex);
        }

        public void ChangeTurn(int nextPlayerIndex) {
            var action = new ChangeTurnAction(this, nextPlayerIndex);
            Container.Perform(action);
        }

        public override void Awake() {
            Subscribe(new EventSubscription<ChangeTurnAction>(OnChangedTurn));
        }

        void OnChangedTurn(ChangeTurnAction action) {
            Container.GetMatch().SetCurrentPlayerIndex(action.TargetPlayerIndex);
        }
    }
}