using System.Collections.Generic;
using GimGim.Serialization;

namespace GimGim.Model {
    /// <summary>
    /// Match class representing a match in the game which
    /// </summary>
    public class Match : ISerializable {
        private int _currentPlayerIndex = 0;
        private int _currentTurn = 0;
        private List<Player> _playerList = new List<Player>(PLAYER_COUNT);
        
        private const int PLAYER_COUNT = 2;
        
        public Match() {
            for (int i = 0; i < PLAYER_COUNT; i++) {
                _playerList.Add(new Player(i));
            }
        }
        
        public int CurrentPlayerIndex {
            get => _currentPlayerIndex;
            set => _currentPlayerIndex = value;
        }

        public Player CurrentPlayer => _playerList[_currentPlayerIndex];

        public Player OpponentPlayer => _playerList[1 - _currentPlayerIndex];
        
        public void Encode(IEncoder coder) {
            coder.Add("currentPlayerIndex", _currentPlayerIndex);
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
