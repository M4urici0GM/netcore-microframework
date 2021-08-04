using System;
using System.Runtime.InteropServices;

namespace tests_socket_net
{

    public struct LogType
    {
        public const string Debug = "Debug";
        public const string Info = "Info";
        public const string Error = "Error";
    }

    public interface ILogger
    {
        void LogInformation(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
    
    public class Logger : ILogger
    {
        private readonly ConsoleColor _debugColor;
        private readonly ConsoleColor _errorColor;
        private readonly ConsoleColor _infoColor;

        public Logger()
        {
            _debugColor = ConsoleColor.Yellow;
            _errorColor =ConsoleColor.Red; ;
            _infoColor = ConsoleColor.Green;
        }

        public Logger(
            ConsoleColor debugColor,
            ConsoleColor errorColor,
            ConsoleColor infoColor)
        {
            _debugColor = debugColor;
            _errorColor = errorColor;
            _infoColor = infoColor;
        }
        
        private void Print(string message, params object[] parameters) => Console.Write(message, parameters);

        private void PrintLn(string message, params object[] parameters) => Console.WriteLine(message, parameters);
        
        private void ResetColors() => Console.ResetColor();

        private void SetFontColor(ConsoleColor consoleColor) => Console.ForegroundColor = consoleColor;

        private void Print(string message, ConsoleColor consoleColor, params object[] parameters)
        {
            SetFontColor(consoleColor);
            Print(message, parameters);
            ResetColors();   
        }
        
        private void PrintLn(string message, ConsoleColor consoleColor, params object[] parameters)
        {
            SetFontColor(consoleColor);
            PrintLn(message, parameters);
            ResetColors();   
        }

        private void PrintLog(string logType, string message, ConsoleColor color)
        {
            Print("[{0}] {1}: ", color, DateTime.UtcNow.ToString("u"), logType);
            PrintLn(message);
        }

        public void LogInformation(string message) => PrintLog(LogType.Info, message, _infoColor);

        public void LogDebug(string message) => PrintLog(LogType.Debug, message, _debugColor);

        public void LogError(string message) => PrintLog(LogType.Error, message, _errorColor);
    }
}