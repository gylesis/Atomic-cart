using System;
using System.IO;
using System.Text;
using Dev.Infrastructure;
using UnityEngine;
using Zenject;

namespace Dev.Utils
{
    public class AtomicLogger : IDisposable, IInitializable
    {
        private static StringBuilder _logs;

        private static bool _isLogging = true;
        private GameSettings _gameSettings;

        public AtomicLogger(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _logs = new StringBuilder();
        }
        
        public void Initialize()
        {
            Application.logMessageReceived += OnLogReceived;
        }
       
        private void OnLogReceived(string msg, string stacktrace, LogType type)
        {
            if (_isLogging == false) return;

            ApplicationOnlogMessageReceived(AtomicConstants.LogTags.Default, msg, type, true);
        }

        private static void ApplicationOnlogMessageReceived(string tag, string msg, LogType type, bool isSilent = false)
        {
            string log = $"{tag} {msg}";

            string msgWithTag = log;

            string timeStamp = $"[{DateTime.Now:HH:mm:ss}]";

            log = log.Insert(0, $"\n{timeStamp}");

            _logs.Append(log);

            if (isSilent) return;

            _isLogging = false;

            switch (type)
            {
                case LogType.Exception:
                case LogType.Error:
                case LogType.Assert:
                    Debug.LogError($"{tag} <color=red>{msg}</color>");
                    break;
                case LogType.Warning:
                    Debug.LogWarning(msgWithTag);
                    break;
                case LogType.Log:
                    Debug.Log(msgWithTag);
                    break;
                default:
                    Debug.Log(msgWithTag);
                    break;
            }

            _isLogging = true;
        }

        public static void Log(string message, string tag = default, bool isSilent = false)
        {
            if (tag == default)
            {
                tag = AtomicConstants.LogTags.Default;
            }

            ApplicationOnlogMessageReceived(tag, message, LogType.Log, isSilent);
        }

        public static void Err(string message, string tag = default, bool isSilent = false)
        {
            if (tag == default)
            {
                tag = AtomicConstants.LogTags.Default;
            }

            ApplicationOnlogMessageReceived(tag, message, LogType.Error, isSilent);
        }

        public static void Ex(string message, string tag = default, bool isSilent = false)
        {
            if (tag == default)
            {
                tag = AtomicConstants.LogTags.Default;
            }

            ApplicationOnlogMessageReceived(tag, message, LogType.Exception, isSilent);
        }
        
        public static void SaveLogs()
        {
            string path = AtomicConstants.SaveLoad.LogSavePath + @"\log.txt";

            string contents = _logs.ToString();

            File.WriteAllText(path, contents);

            Debug.Log($"Saved logs on {path}");
        }
        
        public void Dispose()
        {
            Application.logMessageReceived -= OnLogReceived;

            if (_gameSettings.SaveLogsAfterQuit)
                SaveLogs();
        }

       
    }
}