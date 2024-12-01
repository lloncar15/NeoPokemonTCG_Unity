using System.Collections.Generic;

namespace GimGim.Models {
    public class Pokemon : Card {
        private int _hp;
        private List<EnergyType> _types = new List<EnergyType>();
        private List<string> _evolvesFrom = new List<string>();
        private List<string> _evolvesTo = new List<string>();
        private List<EnergyType> _weaknesses = new List<EnergyType>();
        private List<EnergyType> _resistances = new List<EnergyType>();
        private List<EnergyType> _retreatCost = new List<EnergyType>();
    }
}