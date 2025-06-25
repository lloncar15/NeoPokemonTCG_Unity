using GimGim.AspectContainer;
using GimGim.Model;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// System that handles all the game data and state management for the gameplay.
    /// </summary>
    public class GameDataSystem : GameplaySystem {
        /// <summary>
        /// The main match object that holds the current game state.
        /// </summary>
        public Match Match;

        public override void Awake() {
            Match = new Match();
        }

        public void SaveGame() {
            //TODO: should be connected to a save game system to encode the match data
        }

        public void LoadGame() {
            //TODO: should be connected to a save game system to decode the match data
        }
    }
    
    public static class GameDataSystemExtensions {
        /// <summary>
        /// Gets the current match from the game data system.
        /// </summary>
        /// <param name="game">The game container.</param>
        /// <returns>The current match object.</returns>
        public static Match GetMatch(this IContainer game) {
            GameDataSystem gameDataSystem = game.GetAspect<GameDataSystem>();
            return gameDataSystem.Match;
        }
    }
}