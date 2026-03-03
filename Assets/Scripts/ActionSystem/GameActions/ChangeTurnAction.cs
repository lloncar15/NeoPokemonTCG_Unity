namespace GimGim.ActionSystem {
    public class ChangeTurnAction : GameAction {
        public int TargetPlayerIndex { get; private set; }

        public ChangeTurnAction(object sender, int targetPlayerIndex) : base(sender) {
            TargetPlayerIndex = targetPlayerIndex;
        }
    }
}