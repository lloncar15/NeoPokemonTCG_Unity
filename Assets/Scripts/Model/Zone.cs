using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Model {
    /// <summary>
    /// Zone class representing a zone in the game which holds a list of cards.
    /// </summary>
    /// <typeparam name="T">Type of the zone</typeparam>
    public class Zone<T> : ISerializable where T : Card {
        private List<T> _cards = new List<T>();
        private Zone _zoneType;
        
        public Zone ZoneType => _zoneType;

        public Zone(Zone zoneType, int capacity = 0) {
            if (capacity < 0) {
                throw new System.ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
            }

            if (capacity > 0) {
                _cards.Capacity = capacity;
            }
            
            _zoneType = zoneType;
        }
        
        public void AddCard(T card) => _cards.Add(card);
        public void RemoveCard(T card) => _cards.Remove(card);
        public List<T> GetCards() => new List<T>(_cards);
        
        public void Encode(IEncoder coder) {
            coder.Add("cards", _cards);
            coder.Add("zoneType", _zoneType);
        }

        public bool Decode(IDecoder coder) {
            bool success = true;
            success &= coder.Get("cards", ref _cards);
            success &= coder.Get("zoneType", ref _zoneType);
            return success;
        }
    }
}