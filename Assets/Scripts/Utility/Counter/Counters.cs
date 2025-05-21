using System.Threading;

namespace GimGim.Utility.Counter {
    public class LocalCounter : ICounter {
        private int _counter = -1;
        public int Next() => ++_counter;
        
        public void Reset() => _counter = -1;
    }

    public class ManualCounter : ICounter {
        private int Counter { get; set; } = -1;
        public int Next() => ++Counter;
        public void Advance(int advanceFor = 1) => Counter += advanceFor;
        
        public void Reset() => Counter = -1;
    }

    public class GlobalCounter : ICounter {
        private int _counter = -1;
        public int Next() => Interlocked.Increment(ref _counter);
        
        public void Reset() => Interlocked.Exchange(ref _counter, -1);
    }
}