using System.Threading;

namespace GimGim.Utility.Counter {
    public class LocalCounter : ICounter {
        private int _counter = -1;
        public int Next() => ++_counter;
    }

    public class ManualCounter : ICounter {
        private int Counter { get; set; }
        public int Next() => ++Counter;
        public void Advance(int advanceFor = 1) => Counter += advanceFor;
    }

    public class GlobalCounter : ICounter {
        private int _counter = -1;
        public int Next() => Interlocked.Increment(ref _counter);
    }
}