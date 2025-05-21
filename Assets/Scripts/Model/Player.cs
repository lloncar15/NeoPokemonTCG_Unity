using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Model {
    public class Player : ISerializable {
        private Dictionary<Zone, object> _zones = new();
        
        public int PlayerIndex;
        
        private const int MAX_BENCH_SIZE = 5;
        private const int MAX_ACTIVE_POKEMON_SIZE = 1;
        private const int MAX_STADIUM_SIZE = 1;
        private const int MAX_DECK_SIZE = 60;
        private const int MAX_PRIZES_SIZE = 6;
        public Player(int i) {
            PlayerIndex = i;
            _zones[Zone.Hand] = new Zone<Card>(Zone.Hand);
            _zones[Zone.Deck] = new Zone<Card>(Zone.Deck, MAX_DECK_SIZE);
            _zones[Zone.DiscardPile] = new Zone<Card>(Zone.DiscardPile);
            _zones[Zone.Prizes] = new Zone<Card>(Zone.Prizes, MAX_PRIZES_SIZE);
            _zones[Zone.Bench] = new Zone<PokemonCard>(Zone.Bench, MAX_BENCH_SIZE);
            _zones[Zone.ActivePokemon] = new Zone<PokemonCard>(Zone.ActivePokemon, MAX_ACTIVE_POKEMON_SIZE);
            _zones[Zone.Stadium] = new Zone<TrainerCard>(Zone.Stadium, MAX_STADIUM_SIZE);
        }

        public Zone<T> GetZone<T>(Zone zone) where T : Card => (Zone<T>)_zones[zone];
        
        public void Encode(IEncoder coder) {
            coder.Add("playerIndex", PlayerIndex);
            coder.Add("zones", _zones);
        }

        public bool Decode(IDecoder coder) {
            bool success = true;
            success &= coder.Get("playerIndex", ref PlayerIndex);
            success &= coder.Get("zones", ref _zones);
            return success;
        }
    }
}