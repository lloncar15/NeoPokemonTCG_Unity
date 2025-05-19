using UnityEngine;
using GimGim.Utility.Logger;

namespace GimGim.Config {
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GimGim/Config/GameConfig")]
    public class GameConfig : ScriptableObject {
        [Header("Logging")] 
        [SerializeField] private GameLogger.LogLevel logLevel;
        public GameLogger.LogLevel LogLevel => logLevel;
    }
}