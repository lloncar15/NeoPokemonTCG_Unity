using UnityEngine;

namespace GimGim.Singleton {
    public class Singleton<T> : MonoBehaviour where T : Component {
        private static T _instance;
        
        private static bool HasInstance => _instance != null;
        public static T TryGetInstance() => HasInstance ? _instance : null;

        public static T Instance {
            get {
                if (_instance == null) {
                    _instance = FindAnyObjectByType<T>();
                    if (_instance == null) {
                        var gameObject = new GameObject(typeof(T).Name + "  Auto-Generated");
                        _instance = gameObject.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
        
        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected void Awake() {
            InitializeSingleton();
        }
        
        protected virtual void InitializeSingleton() {
            if (!Application.isPlaying) return;

            _instance = this as T;
        }
    }
}

