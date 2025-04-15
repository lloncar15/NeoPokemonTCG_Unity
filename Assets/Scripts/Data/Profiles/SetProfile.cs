using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public class SetProfile : Profile {
        private string _name;
        private SetSeries _series;
        private int _totalCards;
        private List<string> _images = new List<string>();
        private HashSet<int> _cardProfiles = new HashSet<int>();

        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("series", ref _series);
            success &= decoder.Get("totalCards", ref _totalCards);
            success &= decoder.Get("images", ref _images);

            return success;
        }
        
        public void AddCardProfile(int cardProfileId) {
            _cardProfiles.Add(cardProfileId);
        }
    }
}