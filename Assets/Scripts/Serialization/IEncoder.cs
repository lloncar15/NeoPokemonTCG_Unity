
namespace GimGim.Serialization {
    public interface IEncoder {
        string GetString();
        
        void Add<T>(T value);
        void Add<TK, TV>(TK key, TV value);
    }
}
