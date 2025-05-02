using System.Collections.Generic;
using System.Text;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    /// <summary>
    /// Profile for a Pokemon card.
    /// </summary>
    public class PokemonProfile : CardProfile {
        private List<EnergyType> _types = new List<EnergyType>();
        private List<string> _evolvesFrom = new List<string>();
        private List<string> _evolvesTo = new List<string>();
        private List<Dictionary<string, string>> _weaknesses = new List<Dictionary<string, string>>();
        private List<Dictionary<string, string>> _resistances = new List<Dictionary<string, string>>();
        private List<EnergyType> _retreatCost = new List<EnergyType>();
        private int _convertedRetreatCost = 0;
        private List<Dictionary<string, string>> _abilities = new List<Dictionary<string, string>>(); // TODO: create a class for abilities
        private List<Dictionary<string, string>> _attacks = new List<Dictionary<string, string>>(); // TODO: create a class for attacks
        
        public List<EnergyType> Types => _types;
        public List<string> EvolvesFrom => _evolvesFrom;
        public List<string> EvolvesTo => _evolvesTo;
        public List<Dictionary<string, string>> Weaknesses => _weaknesses;
        public List<Dictionary<string, string>> Resistances => _resistances;
        public List<EnergyType> RetreatCost => _retreatCost;
        public int ConvertedRetreatCost => _convertedRetreatCost;
        public List<Dictionary<string, string>> Abilities => _abilities;
        public List<Dictionary<string, string>> Attacks => _attacks;

        /// <summary>
        /// Decodes and populates the profile using the decoder. Some field in the json can be empty and that's why
        /// success bool doesn't depend on decoding certain fields.
        /// </summary>
        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("types", ref _types);
            decoder.Get("evolvesFrom", ref _evolvesFrom, new List<string>());
            decoder.Get("evolvesTo", ref _evolvesTo, new List<string>());
            decoder.Get("weaknesses", ref _weaknesses, new List<Dictionary<string, string>>());
            decoder.Get("resistances", ref _resistances, new List<Dictionary<string, string>>());
            decoder.Get("retreatCost", ref _retreatCost, new List<EnergyType>());
            decoder.Get("convertedRetreatCost", ref _convertedRetreatCost, 0);
            
            // TODO: create a class for abilities and attacks
            decoder.Get("abilities", ref _abilities, new List<Dictionary<string, string>>());
            decoder.Get("attacks", ref _attacks, new List<Dictionary<string, string>>());
            
            return success;
        }
    }
}