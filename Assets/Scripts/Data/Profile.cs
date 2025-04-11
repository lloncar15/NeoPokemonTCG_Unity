using GimGim.Serialization;

namespace GimGim.Data {
    
    public class Profile : ISerializable {
        public virtual void Encode(IEncoder encoder) {
        }

        public virtual bool Decode(IDecoder decoder) {
            return false;
        }
    }
}