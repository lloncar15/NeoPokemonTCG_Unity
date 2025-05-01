using GimGim.Serialization;

namespace GimGim.Data {
    /// <summary>
    /// Base class for any profile in the game which can be differentiated by an id.
    /// </summary>
    public class Profile : ISerializable {
        
        private int _id;
        public int Id {
            get => _id;
            set => _id = value;
        }
        
        public virtual void Encode(IEncoder encoder) {
            encoder.Add("id", _id);
        }

        public virtual bool Decode(IDecoder decoder) {
            return decoder.Get("id", ref _id);
        }
    }
}