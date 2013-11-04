using System;
using System.Threading;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HatlessEngine
{
    public static class Log
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        public static bool ConsoleEnabled { get; private set; }

        public static bool FileEnabled { get; private set; }

        static Log()
        {
            ConsoleEnabled = false;
            FileEnabled = false;
        }

        public static void Message(string message, ErrorLevel errorLevel = ErrorLevel.DEBUG)
        {
            if (errorLevel != ErrorLevel.NONE)
            {
                message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + errorLevel.ToString() + "] " + message;
            }

            WriteConsole(message);
            //WriteFile(message);

            if (errorLevel == ErrorLevel.FATAL)
            {
                WriteConsole("Shutting down in 5...");
                //WriteFile("Shutting down in 5...");
                Thread.Sleep(4750);
                Environment.Exit(1);
            }
        }

        //console control
        public static void EnableConsole()
        {
            AllocConsole();
            Console.Title = "HatlessEngine Console";
            ConsoleEnabled = true;
        }
        public static void ClearConsole()
        {
            Console.Clear();
        }
        public static void WriteConsole(string message)
        {
            if (ConsoleEnabled)
            {
                Console.WriteLine(message);
            }
        }
    }
}
