using System;
using System.Threading;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace HatlessEngine
{
    public static class Log
    {
        public static bool ConsoleEnabled { get; private set; }
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        public static bool FileEnabled { get; private set; }
        internal static TextWriter FileWriter;


        static Log()
        {
            ConsoleEnabled = false;
            FileEnabled = false;
        }

        public static void Message(string message, ErrorLevel errorLevel = ErrorLevel.DEBUG)
        {
            string formattedMessage;

            if (errorLevel != ErrorLevel.NONE)
            {
                formattedMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + errorLevel.ToString() + "] " + message;
            }
            else
            {
                formattedMessage = message;
            }

            WriteConsole(formattedMessage);
            WriteFile(formattedMessage);

            if (errorLevel == ErrorLevel.FATAL)
            {
                MessageBox.Show(message, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Game.Exit();
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

        //file control
        public static void EnableFile(string logFile)
        {
            if (!File.Exists(logFile))
                FileWriter = new StreamWriter(File.OpenWrite(logFile));
            else
                FileWriter = File.AppendText(logFile);
            FileEnabled = true;
        }
        public static void WriteFile(string message)
        {
            if (FileEnabled)
            {
                FileWriter.WriteLine(message);
            }
        }
        public static void DisableFile()
        {
            if (FileEnabled)
            {
                FileWriter.Close();
                FileEnabled = false;
            }
        }
    }
}
