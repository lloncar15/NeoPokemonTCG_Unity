using GimGim.Data;

namespace GimGim.Model {
    public abstract class Card {
        private readonly CardProfile _profile;

        protected Card(CardProfile profile) {
            _profile = profile;
        }

        protected virtual CardProfile GetProfile() {
            return _profile;
        }
    }
    
    public class PokemonCard : Card {
        public PokemonCard(CardProfile profile) : base(profile) {
        }

        protected override CardProfile GetProfile() {
            return (PokemonProfile)base.GetProfile();
        }
    }

    public class TrainerCard : Card {
        public TrainerCard(CardProfile profile) : base(profile) {
        }

        protected override CardProfile GetProfile() {
            return (TrainerProfile)base.GetProfile();
        }
    }
    
    public class EnergyCard : Card {
        public EnergyCard(CardProfile profile) : base(profile) {
        }

        protected override CardProfile GetProfile() {
            return (EnergyProfile)base.GetProfile();
        }
    }
}