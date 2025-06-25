namespace GimGim.ActionSystem.GameActions {
    public class ChangeTurnAction : GameAction {
        public int TargetPlayerIndex { get; private set; }

        public ChangeTurnAction(int targetPlayerIndex, object sender = null) : base(sender) {
            TargetPlayerIndex = targetPlayerIndex;
        }
    }
}