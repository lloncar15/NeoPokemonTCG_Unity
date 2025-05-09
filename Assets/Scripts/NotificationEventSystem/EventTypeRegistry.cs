using System;
using System.Collections.Generic;
using GimGim.Utility;
using UnityEditor;
using UnityEngine;

namespace GimGim.NotificationEventSystem {
    /// <summary>
    /// A static utility class responsible for managing and caching type hashes for event data types.
    /// This is used to efficiently identify and process event types in the event system.
    /// </summary>
    public static class EventTypeRegistry {
        /// <summary>
        /// A cache that stores precomputed type hashes for each type.
        /// </summary>
        private static readonly Dictionary<Type, HashSet<int>> TypeHashCache = new();

        /// <summary>
        /// Retrieves the set of type hashes for a given type. If the hashes are not already cached,
        /// they are calculated and stored in the cache.
        /// </summary>
        /// <param name="type">The type for which to retrieve the hashes.</param>
        /// <returns>A set of integer hashes representing the type and its hierarchy.</returns>
        public static HashSet<int> GetTypeHashes(Type type) {
            if (TypeHashCache.TryGetValue(type, out HashSet<int> hashes)) {
                return hashes;
            }

            hashes = CalculateTypeHashes(type);
            TypeHashCache[type] = hashes;
            return hashes;
        }
        
        /// <summary>
        /// Calculates the type hashes for a given type, including its base types and implemented interfaces.
        /// </summary>
        /// <param name="type">The type for which to calculate the hashes.</param>
        /// <returns>A set of integer hashes representing the type and its hierarchy.</returns>
        private static HashSet<int> CalculateTypeHashes(Type type) {
            HashSet<int> hashes = new HashSet<int>();
            Stack<Type> stack = new Stack<Type>();
            stack.Push(type);

            while (stack.Count > 0) {
                Type current = stack.Pop();
                if (current == null || current == typeof(object) || current == typeof(UnityEngine.Object)) {
                    continue;
                }

                if (!hashes.Add(HashUtility.GenerateSha1Hash(current.FullName))) {
                    continue;
                }

                if (current.BaseType is not null) {
                    stack.Push(current.BaseType);
                }

                foreach (Type interfaceType in current.GetInterfaces()) {
                    stack.Push(interfaceType);
                }
            }

            return hashes;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Clears the type hash cache when exiting play mode in the Unity Editor.
        /// This ensures that cached data does not persist between play sessions.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void EditorResetOnExitPlayMode() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {
                    TypeHashCache.Clear();
                    Debug.Log("Cleared events!");
                }
            };
        }
        #endif

        /// <summary>
        /// Preloads all event data types and their hashes at runtime before the scene loads.
        /// This ensures that all event types are registered and ready for use.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadEventDataTypes() {
            Type eventDataType = typeof(Event);
            List<Type> types = PredefinedAssemblyUtility.GetTypes(eventDataType);

            foreach (Type type in types) {
                GetTypeHashes(type);
            }
            Debug.Log($"Preloaded {types.Count} event data types.");
        }
    }
}