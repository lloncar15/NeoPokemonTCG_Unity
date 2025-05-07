using System.Collections.Generic;

namespace GimGim.EventCenter {
    /// <summary>
    /// Base abstract class for game event data objects.
    /// </summary>
    public abstract class EventData {
        private object Sender { get; set; }
        public HashSet<int> TypeHashes { get; set; }

        public void SetSender(object sender) {
            Sender = sender;
            TypeHashes = EventTypeRegistry.GetTypeHashes(GetType());
        }

        public override string ToString() => $"{GetType().Name} (Sender: {Sender?.GetType().Name ?? "null"})";
    }
}