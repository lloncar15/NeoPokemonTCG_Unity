using System;
using System.Collections.Generic;
using System.Linq;
using GimGim.Utility;
using GimGim.Utility.Logger;
using UnityEditor;
using UnityEngine;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Registry for GameAction event types that extends <see cref="TypeRegistry"/> functionality.
    /// Manages creation and caching of GameAction-specific event types.
    /// </summary>
    public static class GameActionTypeRegistry {
        /// <summary>
        /// Cache storing GameAction event types and posting delegates
        /// </summary>
        private static readonly Dictionary<Type, GameActionEventTypes> EventTypeCache = new();

        /// <summary>
        /// Gets or creates a <see cref="GameActionEventTypes"/> instance for the specified event type.
        /// </summary>
        public static GameActionEventTypes GetEventTypes(Type eventType) {
            if (EventTypeCache.TryGetValue(eventType, out GameActionEventTypes types)) {
                return types;
            }
            
            types = new GameActionEventTypes(eventType);
            EventTypeCache[eventType] = types;
            
            RegisterEventTypesWithTypeRegistry(types);
            
            GameLogger.InfoWithFormat("Registered event types for GameAction: {0}", eventType.Name);
            
            return types;
        }

        /// <summary>
        /// Registers all event types of the specified <see cref="GameAction"/> with <see cref="TypeRegistry"/>.
        /// </summary>
        private static void RegisterEventTypesWithTypeRegistry(GameActionEventTypes types) {
            // Ensure all event types are in the TypeRegistry hash cache
            TypeRegistry.GetTypeHashes(types.PreparedEventType);
            TypeRegistry.GetTypeHashes(types.PerformedEventType);
            TypeRegistry.GetTypeHashes(types.CanceledEventType);
            TypeRegistry.GetTypeHashes(types.CompletedEventType);
            TypeRegistry.GetTypeHashes(types.FlowStartedEventType);
            TypeRegistry.GetTypeHashes(types.FlowCompletedEventType);
        }
        
        /// <summary>
        /// Preloads all GameAction types at startup
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PreloadGameActionTypes() {
            // Get all GameAction types using PredefinedAssemblyUtility
            var gameActionTypes = PredefinedAssemblyUtility.GetTypes(typeof(GameAction));
            
            // Also ensure interface types are registered
            RegisterInterfaceTypes();
            
            // Preload event types for each concrete GameAction
            int count = 0;
            foreach (Type actionType in gameActionTypes.Where(actionType => !actionType.IsAbstract)) {
                GetEventTypes(actionType);
                count++;
            }
            
            GameLogger.InfoWithFormat("Preloaded {0} GameAction event types", count);
        }
        
        /// <summary>
        /// Registers interface types used for general event subscriptions
        /// </summary>
        private static void RegisterInterfaceTypes() {
            // Register all GameAction event interfaces with TypeRegistry
            TypeRegistry.GetTypeHashes(typeof(IGameActionPreparedEvent));
            TypeRegistry.GetTypeHashes(typeof(IGameActionPerformedEvent));
            TypeRegistry.GetTypeHashes(typeof(IGameActionCanceledEvent));
            TypeRegistry.GetTypeHashes(typeof(IGameActionCompletedEvent));
            TypeRegistry.GetTypeHashes(typeof(IGameActionFlowStartedEvent));
            TypeRegistry.GetTypeHashes(typeof(IGameActionFlowCompletedEvent));
            
            GameLogger.InfoWithFormat("Registered GameAction event interfaces");
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Clears the event type cache when exiting play mode in the Unity Editor
        /// </summary>
        [InitializeOnLoadMethod]
        private static void EditorResetOnExitPlayMode() {
            EditorApplication.playModeStateChanged += state => {
                if (state == PlayModeStateChange.ExitingPlayMode) {
                    EventTypeCache.Clear();
                    GameLogger.InfoWithFormat("Cleared GameAction event type cache");
                }
            };
        }
        #endif
    }
}