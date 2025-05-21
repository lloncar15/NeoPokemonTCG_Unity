using GimGim.Data;

namespace GimGim.Model {
    public class PokemonCard : Card {
        public PokemonCard(int profileId) : base(profileId) {
        }

        protected override CardProfile GetProfile() {
            return (PokemonProfile)base.GetProfile();
        }
    }
}