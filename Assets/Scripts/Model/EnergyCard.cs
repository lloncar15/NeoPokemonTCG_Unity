using GimGim.Data;

namespace GimGim.Model {
    public class EnergyCard : Card {
        public EnergyCard(int profileId) : base(profileId) {
        }

        protected override CardProfile GetProfile() {
            return (EnergyProfile)base.GetProfile();
        }
    }
}