using System;
using System.IO;

namespace HatlessEngine
{
    public static class Log
    {
        public static bool FileEnabled { get; private set; }
		internal static StreamWriter FileWriter;

        static Log()
        {
            FileEnabled = false;
        }

		public static void Message(string message, LogLevel logLevel = LogLevel.Debug)
        {
            string formattedMessage;

			if (logLevel != LogLevel.None)
            {
                formattedMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + logLevel.ToString() + "] " + message;
            }
            else
            {
                formattedMessage = message;
            }

            WriteConsole(formattedMessage);
            WriteFile(formattedMessage);

			if (logLevel == LogLevel.Fatal)
            {
				//MessageBox.ShowDialog(message, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Game.Exit();
            }
        }

        //console control
        public static void ClearConsole()
        {
            Console.Clear();
        }
        public static void WriteConsole(string message)
        {
        	Console.WriteLine(message);
        }

        //file control
        public static void EnableFile(string logFile)
        {
			FileWriter = new StreamWriter(File.Open(logFile, FileMode.Append, FileAccess.Write, FileShare.Read));
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
