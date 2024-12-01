using System.Collections.Generic;

namespace GimGim.Models {
    public class Player {
        private List<Card> _hand = new List<Card>();
        private List<Card> _deck = new List<Card>();
        private List<Card> _discardPile = new List<Card>();
        private List<Card> _prizes = new List<Card>();
        private List<Card> _bench = new List<Card>();
        private List<Card> _activePokemon = new List<Card>(1);
        private List<Card> _stadium = new List<Card>(1);
        
        public readonly int PlayerIndex;
        public Player(int i) {
            this.PlayerIndex = i;
        }

        public List<Card> this[Zone zone] {
            get {
                switch (zone) {
                    case Zone.Hand:
                        return _hand;
                    case Zone.Deck:
                        return _deck;
                    case Zone.DiscardPile:
                        return _discardPile;
                    case Zone.Prizes:
                        return _prizes;
                    case Zone.Bench:
                        return _bench;
                    case Zone.ActivePokemon:
                        return _activePokemon;
                    case Zone.Stadium:
                        return _stadium;
                    default:
                        return new List<Card>();
                }
            }
        }
    }
}