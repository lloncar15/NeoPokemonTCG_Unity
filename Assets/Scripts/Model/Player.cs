using System.Collections.Generic;
using GimGim.Data;
using GimGim.Enums;

namespace GimGim.Model {
    public class Player {
        private List<CardProfile> _hand = new List<CardProfile>();
        private List<CardProfile> _deck = new List<CardProfile>();
        private List<CardProfile> _discardPile = new List<CardProfile>();
        private List<CardProfile> _prizes = new List<CardProfile>();
        private List<CardProfile> _bench = new List<CardProfile>();
        private List<CardProfile> _activePokemon = new List<CardProfile>(1);
        private List<CardProfile> _stadium = new List<CardProfile>(1);
        
        public readonly int PlayerIndex;
        public Player(int i) {
            this.PlayerIndex = i;
        }

        public List<CardProfile> this[Zone zone] {
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
                        return new List<CardProfile>();
                }
            }
        }
    }
}