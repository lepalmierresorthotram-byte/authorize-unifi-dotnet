using System;
using System.IO;
using System.Configuration;

namespace AuthorizeUnifi.Utils
{
    /// <summary>
    /// Logging utility for application events and errors
    /// </summary>
    public static class Logger
    {
        private static readonly string LogPath = ConfigurationManager.AppSettings["LogPath"] ?? "C:\\Logs\\AuthorizeUnifi\\";

        static Logger()
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory: {ex.Message}");
            }
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            try
            {
                string logFile = Path.Combine(LogPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                
                lock (logFile)
                {
                    File.AppendAllText(logFile, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            Log(message, LogLevel.Info);
        }

        public static void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public static void LogError(string message, Exception ex = null)
        {
            string fullMessage = ex != null ? $"{message} - Exception: {ex.Message}" : message;
            Log(fullMessage, LogLevel.Error);
        }

        public static void LogDebug(string message)
        {
            #if DEBUG
            Log(message, LogLevel.Debug);
            #endif
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
