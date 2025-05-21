using GimGim.Data;

namespace GimGim.Model {
    public class TrainerCard : Card {
        public TrainerCard(int profileId) : base(profileId) {
        }

        protected override CardProfile GetProfile() {
            return (TrainerProfile)base.GetProfile();
        }
    }
}