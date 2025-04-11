using System.Security;

namespace GimGim.Serialization {
    public interface ISerializable {
        void Encode(IEncoder coder);
        bool Decode(IDecoder coder);
    }
}