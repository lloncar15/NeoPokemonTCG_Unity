namespace GimGim.Utility.Counter {
    public interface ICounter {
        /// <summary>
        /// Returns a strictly increasing sequential number on each call.
        /// </summary>
        int Next();

        void Reset();
    }
}