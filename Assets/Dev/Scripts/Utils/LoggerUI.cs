using System;
using System.Text;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dev.Utils
{
    public class LoggerUI : MonoBehaviour, ILogger
    {
        [SerializeField] private TMP_Text _logText;

        private StringBuilder _stringBuilder = new StringBuilder();
        
        private ILogger loggerImplementation;

        public static LoggerUI Instance;
        
        private void Awake()
        {
            Instance = this;
            loggerImplementation = Debug.unityLogger;
        }

        private void Update()
        {
            _logText.text = _stringBuilder.ToString();
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            loggerImplementation.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            loggerImplementation.LogException(exception, context);
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            return loggerImplementation.IsLogTypeAllowed(logType);
        }

        public void Log(LogType logType, object message)
        {
            loggerImplementation.Log(logType, message);
        }

        public void Log(LogType logType, object message, Object context)
        {
            loggerImplementation.Log(logType, message, context);
        }

        public void Log(LogType logType, string tag, object message)
        {
            loggerImplementation.Log(logType, tag, message);
        }

        public void Log(LogType logType, string tag, object message, Object context)
        {
            loggerImplementation.Log(logType, tag, message, context);
        }

        public void Log(object message)
        {
            _stringBuilder.AppendLine(message.ToString());
            loggerImplementation.Log(message);
        }

        public void Log(string tag, object message)
        {
            loggerImplementation.Log(tag, message);
        }

        public void Log(string tag, object message, Object context)
        {
            loggerImplementation.Log(tag, message, context);
        }

        public void LogWarning(string tag, object message)
        {
            loggerImplementation.LogWarning(tag, message);
        }

        public void LogWarning(string tag, object message, Object context)
        {
            loggerImplementation.LogWarning(tag, message, context);
        }

        public void LogError(string tag, object message)
        {
            loggerImplementation.LogError(tag, message);
        }

        public void LogError(string tag, object message, Object context)
        {
            loggerImplementation.LogError(tag, message, context);
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            loggerImplementation.LogFormat(logType, format, args);
        }

        public void LogException(Exception exception)
        {
            loggerImplementation.LogException(exception);
        }

        public ILogHandler logHandler
        {
            get => loggerImplementation.logHandler;
            set => loggerImplementation.logHandler = value;
        }

        public bool logEnabled
        {
            get => loggerImplementation.logEnabled;
            set => loggerImplementation.logEnabled = value;
        }

        public LogType filterLogType
        {
            get => loggerImplementation.filterLogType;
            set => loggerImplementation.filterLogType = value;
        }
    }
}