using System.Collections.Generic;
using GimGim.Enums;

namespace GimGim.Model {
    /// <summary>
    /// Zone class representing a zone in the game which holds a list of cards.
    /// </summary>
    /// <typeparam name="T">Type of the zone</typeparam>
    public class Zone<T> where T : Card {
        private readonly List<T> _cards = new List<T>();
        public Zone ZoneType { get; }

        public Zone(Zone zoneType, int capacity = 0) {
            if (capacity < 0) {
                throw new System.ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
            }

            if (capacity > 0) {
                _cards.Capacity = capacity;
            }

            ZoneType = zoneType;
        }
        
        public void AddCard(T card) => _cards.Add(card);
        public void RemoveCard(T card) => _cards.Remove(card);
        public List<T> GetCards() => new List<T>(_cards);
    }
}