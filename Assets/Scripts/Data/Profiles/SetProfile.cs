using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public class SetProfile : Profile {
        private string _name;
        private string _setCode;
        private SetSeries _series;
        private int _totalCards;
        private int _totalDecks;
        private Dictionary<string, string> _images = new Dictionary<string, string>();
        private HashSet<int> _cardProfiles = new HashSet<int>();
        private HashSet<int> _deckProfiles = new HashSet<int>();
        
        public string Name => _name;
        public string SetCode => _setCode;
        public SetSeries Series => _series;
        public int TotalCards => _totalCards;
        public int TotalDecks => _totalDecks;
        public HashSet<int> CardProfiles => _cardProfiles;
        public HashSet<int> DeckProfiles => _deckProfiles;
        public Dictionary<string, string> Images => _images;
        

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
        
        public void AddCardProfile(int cardProfileId) {
            _cardProfiles.Add(cardProfileId);
        }
        
        public void AddDeckProfile(int deckProfileId) {
            _deckProfiles.Add(deckProfileId);
        }
    }
}