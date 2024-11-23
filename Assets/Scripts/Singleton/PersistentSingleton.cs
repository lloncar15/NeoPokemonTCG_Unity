using System;
using UnityEngine;

namespace GimGim.Singleton {
    /// <summary>
    /// Singleton that persists through scene changes and destroys itself if it created another copy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistentSingleton<T> : MonoBehaviour where T : Component {
        public bool autoRemoveFromParentOnAwake = true;
        
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
            
            if (autoRemoveFromParentOnAwake) {
                transform.SetParent(null);
            }
            
            if (_instance == null) {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else {
                if (_instance != this) {
                    Destroy(gameObject);
                }
            }
        }
    }
}