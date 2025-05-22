using GimGim.AspectContainer;
using GimGim.Utility;

namespace GimGim.GameplaySystems {
    public abstract class GameplaySystem : GameplayAspect, IInitializable {
        public virtual void Awake() {
            SubscribeAll();
        }

        public virtual void Destroy() {
            UnsubscribeAllAndClear();
        }
    }
}