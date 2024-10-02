using System;
using System.IO;

namespace NewGameMode
{
    /// <summary>
    /// 现代化日志系统。
    /// </summary>
    public class YKMTLog
    {
        public bool IsDebugEnabled = false;
        private string LogDir;
        public enum LogLevel
        {
            Info,
            Warn,
            Error,
            Debug
        }
        /// <summary>
        /// 初始化日志系统。
        /// </summary>
        public YKMTLog(string logDir, bool isDebugEnabled)
        {
            if (!Directory.Exists(logDir + "/Logs"))
            {
                Directory.CreateDirectory(Harmony_Patch.path + "/Logs");
            }
            if (isDebugEnabled)
            {
                IsDebugEnabled = true;
            }
            LogDir = logDir;
        }
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageHead">日志头部</param>
        private void AppendLog(string message, string messageHead)
        {
            DateTime currentTime = DateTime.Now;
            File.AppendAllText(LogDir + $"/LobotomyRougeLog-{currentTime:yyyy-MM-dd}.log", $"[{messageHead}][{currentTime:yyyy/MM/dd HH:mm:ss}]" + message + Environment.NewLine);
        }
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="level">日志等级</param>
        public void Log(string message, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Info(message);
                    break;
                case LogLevel.Warn:
                    Warn(message);
                    break;
                case LogLevel.Error:
                    Error(message);
                    break;
                case LogLevel.Debug:
                    Debug(message);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 写入自定义头部的日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageHead">日志头部</param>
        public void Log(string message, string messageHead)
        {
            AppendLog(message, messageHead);
        }
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        /// <param name="level">日志等级</param>
        public void Log(Exception exception, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Info(exception.ToString());
                    break;
                case LogLevel.Warn:
                    Warn(exception.ToString());
                    break;
                case LogLevel.Error:
                    Error(exception.ToString());
                    break;
                case LogLevel.Debug:
                    Debug(exception.ToString());
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 写入自定义头部的日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="messageHead">日志头部</param>
        /// <param name="isDebugLog">是否为调试日志</param>
        public void Log(string message, string messageHead, bool isDebugLog)
        {
            if (isDebugLog && IsDebugEnabled)
            {
                AppendLog(message, messageHead);
            }
        }
        /// <summary>
        /// 写入日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        /// <param name="level">日志等级</param>
        /// <param name="isDebugLog">是否为调试日志</param>
        public void Log(Exception exception, LogLevel level, bool isDebugLog)
        {
            if (isDebugLog && IsDebugEnabled)
            {
                switch (level)
                {
                    case LogLevel.Info:
                        Info(exception.ToString());
                        break;
                    case LogLevel.Warn:
                        Warn(exception.ToString());
                        break;
                    case LogLevel.Error:
                        Error(exception.ToString());
                        break;
                    case LogLevel.Debug:
                        Debug(exception.ToString());
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 写入自定义头部的日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        /// <param name="messageHead">日志头部</param>
        public void Log(Exception exception, string messageHead)
        {
            AppendLog(exception.ToString(), messageHead);
        }
        /// <summary>
        /// 写入信息级别日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            AppendLog(message, "INFO");
        }
        /// <summary>
        /// 写入警告级别日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Warn(string message)
        {
            AppendLog(message, "WARN");
        }
        /// <summary>
        /// 写入错误级别日志。
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message)
        {
            AppendLog(message, "ERROR");
        }
        /// <summary>
        /// 写入调试级别日志。需要开启调试模式才会写入。
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                AppendLog(message, "DEBUG");
            }
        }
        /// <summary>
        /// 写入信息级别日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        public void Info(Exception exception)
        {
            AppendLog(exception.ToString(), "INFO");
        }
        /// <summary>
        /// 写入警告级别日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        public void Warn(Exception exception)
        {
            AppendLog(exception.ToString(), "WARN");
        }
        /// <summary>
        /// 写入错误级别日志。
        /// </summary>
        /// <param name="exception">日志消息</param>
        public void Error(Exception exception)
        {
            AppendLog(exception.ToString(), "ERROR");
        }
        /// <summary>
        /// 写入调试级别日志。需要开启调试模式才会写入。
        /// </summary>
        /// <param name="exception">日志消息</param>
        public void Debug(Exception exception)
        {
            if (IsDebugEnabled)
            {
                AppendLog(exception.ToString(), "DEBUG");
            }
        }
    }
}
