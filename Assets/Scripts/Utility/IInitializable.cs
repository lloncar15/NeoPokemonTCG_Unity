namespace GimGim.Utility {
    /// <summary>
    /// Interface for objects that want to have set up and teardown methods.
    /// </summary>
    public interface IInitializable {
        void Awake();
        void Destroy();
    }
}