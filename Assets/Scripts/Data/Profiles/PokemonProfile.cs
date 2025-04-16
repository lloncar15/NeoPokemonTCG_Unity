using System.Collections.Generic;
using System.Text;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
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

        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("types", ref _types);
            success &= decoder.Get("evolvesFrom", ref _evolvesFrom);
            success &= decoder.Get("evolvesTo", ref _evolvesTo);
            success &= decoder.Get("weaknesses", ref _weaknesses);
            success &= decoder.Get("resistances", ref _resistances);
            success &= decoder.Get("retreatCost", ref _retreatCost);
            success &= decoder.Get("convertedRetreatCost", ref _convertedRetreatCost);
            
            // TODO: create a class for abilities and attacks
            success &= decoder.Get("abilities", ref _abilities);
            success &= decoder.Get("attacks", ref _attacks);
            
            return success;
        }
    }
}