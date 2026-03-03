using GimGim.AspectContainer;
using GimGim.Utility;

namespace GimGim.GameplaySystems {
    /// <summary>
    /// Base abstract class for any gameplay system that will be used during a match.
    /// </summary>
    public abstract class GameplaySystem : GameplayAspect, IInitializable {
        /// <summary>
        /// Subscribes all the events from the subscriptions list.
        /// Should be called from the Unity wrappers OnAwake method.
        /// </summary>
        public virtual void Awake() {
            SubscribeAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual void Destroy() {
            UnsubscribeAllAndClear();
        }
    }
}