namespace GimGim.Serialization {
    public interface ISerializable {
        public bool Encode();
        public bool Decode();
    }
}