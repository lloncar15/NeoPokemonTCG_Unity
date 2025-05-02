using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    /// <summary>
    /// Profile for a deck.
    /// </summary>
    public class DeckProfile : Profile {
        private string _name;
        private string _setCode;
        private int _setId;
        private List<EnergyType> _types = new List<EnergyType>();
        private List<(int cardId, int count)> _cards = new List<(int cardId, int count)>();
        
        public int SetId => _setId;
        
        private const int SetBaseId = 1000;
        
        /// <summary>
        /// Decodes and populates the profile using the decoder. Some field in the json can be empty and that's why
        /// success bool doesn't depend on decoding certain fields.
        /// </summary>
        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("setCode", ref _setCode);
            success &= decoder.Get("types", ref _types);
            success &= decoder.Get("cards", ref _cards);
            
            _setId = (Id / SetBaseId) * SetBaseId;

            return success;
        }
    }
}