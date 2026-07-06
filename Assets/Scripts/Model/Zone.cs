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
        private readonly int _maxSize;

        public Zone ZoneType => _zoneType;
        /// <summary>
        /// Maximum number of cards this zone can hold. Zero means unlimited.
        /// </summary>
        public int MaxSize => _maxSize;
        public int Count => _cards.Count;
        public bool IsFull => _maxSize > 0 && _cards.Count >= _maxSize;

        public Zone(Zone zoneType, int maxSize = 0) {
            if (maxSize < 0) {
                throw new System.ArgumentOutOfRangeException(nameof(maxSize), "Max size cannot be negative.");
            }

            if (maxSize > 0) {
                _cards.Capacity = maxSize;
            }

            _maxSize = maxSize;
            _zoneType = zoneType;
        }

        /// <summary>
        /// Adds a card to the zone. Returns false if the zone is at its maximum size.
        /// </summary>
        public bool AddCard(T card) {
            if (IsFull) return false;
            _cards.Add(card);
            return true;
        }
        public bool RemoveCard(T card) => _cards.Remove(card);
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