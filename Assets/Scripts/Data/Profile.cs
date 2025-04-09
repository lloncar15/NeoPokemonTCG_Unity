using GimGim.Serialization;

namespace GimGim.Data {
    public class Profile : ISerializable {
        public virtual bool Encode() {
            return false;
        }

        public virtual bool Decode() {
            return false;
        }
    }
}