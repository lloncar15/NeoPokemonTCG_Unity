using System.Collections.Generic;
using GimGim.Serialization;

namespace GimGim.Model {
    /// <summary>
    /// Match class representing a match in the game.
    /// </summary>
    public class Match : ISerializable {
        private int _currentPlayerIndex = 0;
        public int CurrentPlayerIndex => _currentPlayerIndex;
        private int _currentTurn = 0;
        private List<Player> _playerList = new(PLAYER_COUNT);
        
        public void SetCurrentPlayerIndex(int index) {
            if (index is >= 0 and < PLAYER_COUNT) {
                _currentPlayerIndex = index;
            }
        }
        
        private const int PLAYER_COUNT = 2;
        
        public Match() {
            for (int i = 0; i < PLAYER_COUNT; i++) {
                _playerList.Add(new Player(i));
            }
        }
        
        public Player CurrentPlayer => _playerList[CurrentPlayerIndex];

        public Player OpponentPlayer => _playerList[1 - CurrentPlayerIndex];
        
        public void Encode(IEncoder coder) {
            coder.Add("currentPlayerIndex", CurrentPlayerIndex);
            coder.Add("currentTurn", _currentTurn);
            coder.Add("playerList", _playerList);
        }

        public bool Decode(IDecoder coder) {
            bool success = true;
            success &= coder.Get("currentPlayerIndex", ref _currentPlayerIndex);
            success &= coder.Get("currentTurn", ref _currentTurn);
            success &= coder.Get("playerList", ref _playerList);
            return success;
        }
    }
}
