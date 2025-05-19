using System;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GimGim.Utility.Logger {
    public class GameLogger {
        public enum LogLevel {
            Verbose,
            Info, 
            Warning, 
            Error, 
            Debug,
            Assert,
            Exception
        }

        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;
        
        private string _tag;
        private Color Color { get; set; }
        private bool ShowTimestamp { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool ShouldLogToFile { get; set; } = false;
        
        private static readonly string LogFilePath = Path.Combine(Application.persistentDataPath, "game-log.txt");

        public static GameLogger Create(string tag, Color color) {
            return new GameLogger() { _tag = tag, Color = color };
        }

        public static GameLogger Create<T>(Color color) {
            return new GameLogger() { _tag = typeof(T).Name, Color = color };
        }

// ReSharper disable Unity.PerformanceAnalysis
        public void LogVerbose(string format, params object[] args) {
            if (Enabled && CurrentLogLevel <= LogLevel.Verbose) {
                string finalFormat = GetFormat(format);
                Debug.LogFormat(finalFormat, args);
                
                LogToFile(finalFormat);
            }
        }


        public void LogInfo(string format, params object[] args) {
            if (Enabled && CurrentLogLevel <= LogLevel.Info) {
                string finalFormat = GetFormat(format);
                Debug.LogFormat(finalFormat, args);
                
                LogToFile(finalFormat);
            }
        }
        
        public void LogWarning(string format, params object[] args) {
            if (Enabled && CurrentLogLevel <= LogLevel.Warning) {
                string finalFormat = GetFormat(format);
                Debug.LogFormat(finalFormat, args);
                
                LogToFile(finalFormat);
            }
        }
        
        public void LogError(string format, params object[] args) {
            if (Enabled && CurrentLogLevel <= LogLevel.Error) {
                string finalFormat = GetFormat(format);
                Debug.LogFormat(finalFormat, args);
                
                LogToFile(finalFormat);
            }
        }
        
        public void LogAssert(UnityEngine.Object context, string format, params object[] args) {
            if (Enabled && CurrentLogLevel <= LogLevel.Assert) {
                string finalFormat = GetFormat(format);
                if (context is not null) {
                    Debug.LogAssertionFormat(context, finalFormat, args);
                }
                else {
                    Debug.LogAssertionFormat(finalFormat, args);
                }
                
                LogToFile(finalFormat);
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context = null) {
            if (Enabled && CurrentLogLevel <= LogLevel.Exception)
            {
                if (context)
                {
                    Debug.LogException(exception, context);
                }
                else
                {
                    Debug.LogException(exception);
                }
            }
        }

        private void LogToFile(string message) {
            if (!ShouldLogToFile) return;
            
            try
            {
                File.AppendAllText(LogFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"GameLogger failed to write to file: {ex.Message}");
            }
        }

        private string GetFormat(string format) {
            StringBuilder builder = new StringBuilder();
            if (ShowTimestamp) {
                builder.Append($"[{DateTime.Now:dd-MM-yyyy HH:mm:ss.fff}]");
            }
            builder.Append("[<color=#")
                .Append(ColorUtility.ToHtmlStringRGB(Color))
                .Append(">")
                .Append(_tag)
                .Append("</color>]")
                .Append(format);
            
            return builder.ToString();
        }
        
        #region Global logger

        private static GameLogger _instance;
        public static GameLogger Instance {
            get { return _instance ??= Create("GlobalLog", new Color(0.65f,0.65f,0.65f)); }
        }
        
        public static void VerboseWithFormat(string format, params object[] args) {
            Instance.LogVerbose(format, args);
        }
        
        public static void InfoWithFormat(string format, params object[] args) {
            Instance.LogInfo(format, args);
        }
        
        public static void WarningWithFormat(string format, params object[] args) {
            Instance.LogWarning(format, args);
        }
        
        public static void ErrorWithFormat(string format, params object[] args) {
            Instance.LogError(format, args);
        }
        
        public static void AssertWithFormat(string format, params object[] args) {
            Instance.LogAssert(null, format, args);
        }

        #endregion
    }
}