using GimGim.Serialization;
using GimGim.Data;
using GimGim.Enums;

namespace GimGim.Model {
    public abstract class Card : ISerializable {
        private int _profileId;
        public int PlayOrder = int.MaxValue;
        public int OwnerIndex = -1;
        public Zone Zone;

        protected Card(int profileId) {
            _profileId = profileId;
        }

        protected virtual CardProfile GetProfile() {
            return ProfilesController.GetProfile<CardProfile>(_profileId);
        }

        public virtual void Encode(IEncoder coder) {
            coder.Add("profileId", _profileId);
            coder.Add("playOrder", PlayOrder);
            coder.Add("ownerIndex", OwnerIndex);
            coder.Add("zone", Zone);
        }

        public virtual bool Decode(IDecoder coder) {
            bool success = true;
            success &= coder.Get("profileId", ref _profileId);
            success &= coder.Get("playOrder", ref PlayOrder);
            success &= coder.Get("ownerIndex", ref OwnerIndex);
            success &= coder.Get("zone", ref Zone);
            return success;
        }
    }
}