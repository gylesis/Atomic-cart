using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Dev.Utils
{
    public class AtomicLogger : IDisposable
    {
        private static StringBuilder _logs;

        private static bool _isLogging = true;
        
        public AtomicLogger()
        {
            _logs = new StringBuilder();
            
            Application.logMessageReceived += OnLogReceived;
        }

        public void Dispose()
        {
            Application.logMessageReceived -= OnLogReceived;
            SaveLogs();
        }

        private void OnLogReceived(string msg, string stacktrace, LogType type)
        {
            if(_isLogging == false) return;
            
            ApplicationOnlogMessageReceived(AtomicConstants.LogTags.Default, msg, type, true);
        }

        private static void ApplicationOnlogMessageReceived(string tag, string msg, LogType type, bool isSilent = false)
        {
            string log = $"{tag} {msg}";  
            
            string msgWithTag = log;
            
            string timeStamp = $"[{DateTime.Now:HH:mm:ss}]";
            
            log = log.Insert(0,$"\n{timeStamp}");

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

        public static void SaveLogs()
        {
            string path = AtomicConstants.SaveLoad.LogSavePath + "\\log.txt";

            string contents = _logs.ToString();
            
            File.WriteAllText(path, contents);

            Debug.Log($"Saved logs on {path}");
        }

        public static void Log(string message, string tag, bool isSilent = false)
        {   
            ApplicationOnlogMessageReceived(tag,message, LogType.Log);
        }

        public static void Err(string message, string tag, bool isSilent = false)
        {
            ApplicationOnlogMessageReceived(tag,message,LogType.Error);
        }

        public static void Ex(string message, string tag, bool isSilent = false)
        {
            ApplicationOnlogMessageReceived(tag,message, LogType.Exception);
        }
    }
}