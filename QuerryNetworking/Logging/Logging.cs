using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Logging
{
    public static class Log
    {
        public static LogLevel LogLevel;
        static List<LogEntry> LogQueue = new List<LogEntry>();
        public static void Debug(string Message)
        {
            if (LogLevel < LogLevel.Debug)
            {
                return;
            }
            
            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.DarkGray,
                Message = "[QuerryNetworking] {debug} " + Message
            });
            WriteQueue();
        }

        public static void Complete(string Message)
        {
            if (LogLevel < LogLevel.Info)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Green,
                Message = "[QuerryNetworking] {complete} " + Message
            });
            WriteQueue();
        }

        public static void Warn(string Message)
        {
            if (LogLevel < LogLevel.Warn)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Yellow,
                Message = "[QuerryNetworking] {warn} " + Message
            });
            WriteQueue();
        }

        public static void Error(string Message)
        {
            if (LogLevel < LogLevel.Error)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Red,
                Message = "[QuerryNetworking] {error} " + Message
            });
            WriteQueue();
        }

        public static void Info(string Message)
        {
            if (LogLevel < LogLevel.Info)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.White,
                Message = "[QuerryNetworking] {info} " + Message
            });
            WriteQueue();
        }

        static bool Logging;
        static void WriteQueue()
        {
            if(Logging == true)
            {
                return;
            }
            Logging = true;
            while(LogQueue.Count > 0)
            {
                Console.ForegroundColor = LogQueue[0].Color;
                Console.WriteLine(LogQueue[0].Message);
                LogQueue.RemoveAt(0);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Logging = false;
        }
    }
    class LogEntry
    {
        public string Message { get; set; } = "";
        public ConsoleColor Color { get; set; }
    }
    public enum LogLevel 
    {
        None,
        Error,
        Warn,
        Info,
        Debug,
    }
}
