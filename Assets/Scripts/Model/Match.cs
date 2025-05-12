using System.Collections.Generic;

namespace GimGim.Model {
    /// <summary>
    /// Match class representing a match in the game which
    /// </summary>
    public class Match {
        private readonly int _currentPlayerIndex = 0;
        private int _currentTurn = 0;
        private readonly List<Player> _playerList = new List<Player>(PLAYER_COUNT);
        
        private const int PLAYER_COUNT = 2;
        
        public Match() {
            for (int i = 0; i < PLAYER_COUNT; i++) {
                _playerList.Add(new Player(i));
            }
        }
        
        public Player CurrentPlayer => _playerList[_currentPlayerIndex];

        public Player OpponentPlayer => _playerList[1 - _currentPlayerIndex];
    }
}
