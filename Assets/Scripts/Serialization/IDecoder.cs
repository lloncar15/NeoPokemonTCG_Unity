namespace GimGim.Serialization {
    public interface IDecoder {
        bool Get<TK, TV>(TK key, ref TV value, TV defaultValue = default);
    }
}