using System;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
	/// <summary>
	/// Contains methods to write (log) messages in a standardized way.
	/// </summary>
	public static class Log
	{
		private static bool ConsoleEnabled = false;
		private static TextWriter ConsoleWriter;

		private static bool FileEnabled = false;
		private static StreamWriter FileWriter;

		public static List<StreamWriter> CustomStreams = new List<StreamWriter>();

		public static void Message(string message, LogLevel logLevel = LogLevel.Debug, bool timeStamp = true)
		{
			string formattedMessage = "";

			if (timeStamp)
				formattedMessage += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";

			if (logLevel != LogLevel.None)
				formattedMessage += "[" + logLevel.ToString() + "] ";

			formattedMessage += message;

			if (ConsoleEnabled)
				ConsoleWriter.WriteLine(formattedMessage);
			if (FileEnabled)
				FileWriter.WriteLine(formattedMessage);
			foreach (StreamWriter stream in CustomStreams)
				stream.WriteLine(formattedMessage);
		}

		public static void EnableConsole()
		{
			if (!ConsoleEnabled)
			{
				ConsoleWriter = Console.Out;
				ConsoleEnabled = true;
			}
		}
		public static void DisableConsole()
		{
			if (ConsoleEnabled)
			{
				ConsoleWriter.Close();
				ConsoleWriter = null;
				ConsoleEnabled = false;
			}
		}

		public static void EnableFile(string filename)
		{
			if (!FileEnabled)
			{
				FileWriter = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read));
				FileEnabled = true;
			}
		}
		public static void DisableFile()
		{
			if (FileEnabled)
			{
				FileWriter.Close();
				FileWriter = null;
				FileEnabled = false;
			}
		}

		/// <summary>
		/// For cleanup before exiting.
		/// </summary>
		internal static void CloseAllStreams()
		{
			DisableConsole();
			DisableFile();
			foreach (StreamWriter stream in CustomStreams)
				stream.Close();
			CustomStreams.Clear();
		}
	}

	public enum LogLevel
	{
		None = 0,
		Debug = 1,
		Notice = 2,
		Warning = 3,
		Critical = 4,
		Fatal = 5
	}
}