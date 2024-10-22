using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Logging
{
    public class Log
    {
        public static LogLevel LogLevel;
        static List<LogEntry> LogQueue = new List<LogEntry>();
        public Log(string Id)
        {
            ServerId = Id;
        }

        string ServerId = "";
        public void Debug(string Message)
        {
            if (LogLevel < LogLevel.Debug)
            {
                return;
            }
            
            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.DarkGray,
                Message = $"[{ServerId}] {{debug}} " + Message
            });
            WriteQueue();
        }

        public void Complete(string Message)
        {
            if (LogLevel < LogLevel.Info)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Green,
                Message = $"[{ServerId}] {{complete}} " + Message
            });
            WriteQueue();
        }

        public void Warn(string Message)
        {
            if (LogLevel < LogLevel.Warn)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Yellow,
                Message = $"[{ServerId}] {{warn}} " + Message
            });
            WriteQueue();
        }

        public void Error(string Message)
        {
            if (LogLevel < LogLevel.Error)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.Red,
                Message = $"[{ServerId}] {{error}} " + Message
            });
            WriteQueue();
        }

        public void Info(string Message)
        {
            if (LogLevel < LogLevel.Info)
            {
                return;
            }

            LogQueue.Add(new LogEntry()
            {
                Color = ConsoleColor.White,
                Message = $"[{ServerId}] {{info}} " + Message
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

            while (LogQueue.Count > 0)
            {
                LogEntry? Entry = null;
                try
                {
                    if (LogQueue.Count > 0)
                    {
                        Entry = LogQueue[0];
                        LogQueue.RemoveAt(0);
                    }
                    if (Entry == null)
                        continue;
                    Console.ForegroundColor = Entry.Color;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm tt") + " - " + Entry.Message);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now.ToString("hh:mm tt") + " - log failure");
                }
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
