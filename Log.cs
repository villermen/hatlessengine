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
		private static bool _consoleEnabled = false;
		private static TextWriter _consoleWriter;

		private static bool _fileEnabled = false;
		private static StreamWriter _fileWriter;

		public static List<StreamWriter> CustomStreams = new List<StreamWriter>();

		public static void Message(string message, LogLevel logLevel = LogLevel.Debug, bool timeStamp = true)
		{
			string formattedMessage = "";

			if (timeStamp)
				formattedMessage += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ";

			if (logLevel != LogLevel.None)
				formattedMessage += "[" + logLevel + "] ";

			formattedMessage += message;

			if (_consoleEnabled)
				_consoleWriter.WriteLine(formattedMessage);
			if (_fileEnabled)
				_fileWriter.WriteLine(formattedMessage);
			foreach (StreamWriter stream in CustomStreams)
				stream.WriteLine(formattedMessage);
		}

		public static void EnableConsole()
		{
			if (_consoleEnabled) 
				return;

			_consoleWriter = Console.Out;
			_consoleEnabled = true;
		}
		public static void DisableConsole()
		{
			if (!_consoleEnabled) 
				return;

			_consoleWriter.Close();
			_consoleWriter = null;
			_consoleEnabled = false;
		}

		public static void EnableFile(string filename)
		{
			if (_fileEnabled) 
				return;

			_fileWriter = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.Read));
			_fileEnabled = true;
		}
		public static void DisableFile()
		{
			if (!_fileEnabled) 
				return;

			_fileWriter.Close();
			_fileWriter = null;
			_fileEnabled = false;
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