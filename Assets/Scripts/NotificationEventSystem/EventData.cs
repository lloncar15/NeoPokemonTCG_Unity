using System.Collections.Generic;
using GimGim.Utility;

namespace GimGim.EventSystem {
    /// <summary>
    /// Base abstract class for game event data objects.
    /// </summary>
    public abstract class EventData : IEvent {
        public object Sender { get; set; }
        public HashSet<int> TypeHashes { get; set; }

        public EventData(object sender) {
            Sender = sender;
            TypeHashes = TypeRegistry.GetTypeHashes(GetType());
        }

        public override string ToString() => $"{GetType().Name} (Sender: {Sender?.GetType().Name ?? "null"})";
    }
    
    public interface IEvent {}
}