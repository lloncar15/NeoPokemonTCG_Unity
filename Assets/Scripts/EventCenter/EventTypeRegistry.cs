using System;
using System.Collections.Generic;
using GimGim.Utility;
using UnityEditor;
using UnityEngine;

namespace GimGim.EventCenter {
    public static class EventTypeRegistry {
        private static readonly Dictionary<Type, HashSet<int>> TypeHashCache = new();

        public static HashSet<int> GetTypeHashes(Type type) {
            if (TypeHashCache.TryGetValue(type, out HashSet<int> hashes)) {
                return hashes;
            }

            hashes = CalculateTypeHashes(type);
            TypeHashCache[type] = hashes;
            return hashes;
        }
        
        private static HashSet<int> CalculateTypeHashes(Type type) {
            HashSet<int> hashes = new HashSet<int>();
            Stack<Type> stack = new Stack<Type>();
            stack.Push(type);

            while (stack.Count > 0) {
                Type current = stack.Pop();
                if (current == null || current == typeof(object) || current == typeof(UnityEngine.Object)) {
                    continue;
                }

                if (!hashes.Add(current.GetHashCode())) {
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadEventDataTypes() {
            Type eventDataType = typeof(EventData);
            List<Type> types = PredefinedAssemblyUtility.GetTypes(eventDataType);

            foreach (Type type in types) {
                GetTypeHashes(type);
            }
            Debug.Log($"Preloaded {types.Count} event data types.");
        }
    }
}