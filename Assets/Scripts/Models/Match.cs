using System.Collections.Generic;

namespace GimGim.Models {
    public class Match {
        public const int PlayerCount = 2;
        
        private int _currentPlayerIndex = 0;
        private int _currentTurn = 0;
        public List<Player> playerList = new List<Player>(PlayerCount);
        
        public Match() {
            for (int i = 0; i < PlayerCount; i++) {
                playerList.Add(new Player(i));
            }
        }
        
        public Player CurrentPlayer {
            get {
                return playerList[_currentPlayerIndex];
            }
        }

        public Player OpponentPlayer {
            get {
                return playerList[1 - _currentPlayerIndex];
            }
        }
    }
}
