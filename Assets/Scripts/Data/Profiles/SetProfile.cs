using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    /// <summary>
    /// Profile for a set of cards, which also contains all the ids of the card profiles and
    /// deck profiles that are tied to the set.
    /// </summary>
    public class SetProfile : Profile {
        private string _name;
        private string _setCode;
        private SetSeries _series;
        private int _totalCards;
        private int _totalDecks;
        private Dictionary<string, string> _images = new Dictionary<string, string>();
        
        private readonly HashSet<int> _cardProfiles = new HashSet<int>();
        private readonly HashSet<int> _deckProfiles = new HashSet<int>();
        
        public string Name => _name;
        public string SetCode => _setCode;
        public SetSeries Series => _series;
        public int TotalCards => _totalCards;
        public int TotalDecks => _totalDecks;
        public HashSet<int> CardProfiles => _cardProfiles;
        public HashSet<int> DeckProfiles => _deckProfiles;
        public Dictionary<string, string> Images => _images;
        
        /// <summary>
        /// Decodes and populates the profile using the decoder. Some field in the json can be empty and that's why
        /// success bool doesn't depend on decoding certain fields.
        /// </summary>
        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("setCode", ref _setCode);
            success &= decoder.Get("series", ref _series);
            success &= decoder.Get("totalCards", ref _totalCards);
            success &= decoder.Get("images", ref _images);
            success &= decoder.Get("totalDecks", ref _totalDecks);

            return success;
        }
        
        /// <summary>
        /// Adds a card profile id to the set profile during profile loading.
        /// </summary>
        /// <param name="cardProfileId"></param>
        public void AddCardProfile(int cardProfileId) {
            _cardProfiles.Add(cardProfileId);
        }
        
        /// <summary>
        /// Adds a deck profile id to the set profile during profile loading.
        /// </summary>
        /// <param name="deckProfileId"></param>
        public void AddDeckProfile(int deckProfileId) {
            _deckProfiles.Add(deckProfileId);
        }
    }
}