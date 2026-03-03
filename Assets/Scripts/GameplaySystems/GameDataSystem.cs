using GimGim.Model;
using GimGim.AspectContainer;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// System that handles all the match data modifications.
    /// </summary>
    public class GameDataSystem : GameplaySystem
    {
        /// <summary>
        /// The main match data.
        /// </summary>
        public Match Match;

        public override void Awake() {
            Match = new Match();
        }

        public void SaveGame() {
            //TODO: should be connected to a savegame controller to encode the match
        }

        public void LoadMatch(string matchJSONString) {
            //TODO: should be connected to a savegame controller to decode a given match
            
        }
    }

    public static class GameplaySystemExtensions {
        public static Match GetMatch(this IContainer game) {
            return game.GetAspect<GameDataSystem>().Match;
        }
    }
}