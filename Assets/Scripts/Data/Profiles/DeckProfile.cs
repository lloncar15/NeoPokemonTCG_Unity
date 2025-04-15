using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public class DeckProfile : Profile {
        private string _name;
        private HashSet<EnergyType> _types = new HashSet<EnergyType>();
        private HashSet<(int cardId, int count)> _cards = new HashSet<(int cardId, int count)>();
        
        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("types", ref _types);
            success &= decoder.Get("cards", ref _cards);

            return success;
        }
    }
}