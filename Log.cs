using System;
using System.Threading;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;

namespace HatlessEngine
{
    public static class Log
    {

        public static bool ConsoleEnabled { get; private set; }
        private static RenderWindow ConsoleWindow;
        private static List<Text> ConsoleMessages = new List<Text>(50);
        private static SFML.Graphics.Font ConsoleFont;
        private static int ConsoleFontLineSpacing;
        private static Color ConsoleTextColor = new Color(192, 192, 192);

        public static bool FileEnabled { get; private set; }

        static Log()
        {
            ConsoleFont = new SFML.Graphics.Font(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("HatlessEngine.Inconsolata.otf"));
            ConsoleFontLineSpacing = ConsoleFont.GetLineSpacing(14);
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
                WriteConsole("Shutting down in 3...");
                //WriteFile("Shutting down in 3...");
                Thread.Sleep(2750);
                Environment.Exit(1);
            }
        }

        private static void ConsoleWindowClosed(object sender, EventArgs e)
        {
            DisableConsole();
        }
        private static void ConsoleWindowResized(object sender, SizeEventArgs e)
        {
            SFML.Graphics.View newView = ConsoleWindow.GetView();
            newView.Size = new Vector2f(e.Width, e.Height);
            newView.Center = new Vector2f(e.Width / 2, e.Height / 2);
            ConsoleWindow.SetView(newView);
            UpdateConsole();
        }

        //console control
        public static void EnableConsole()
        {
            ConsoleWindow = new RenderWindow(new VideoMode(640, 320), "HatlessEngine Console Window");
            ConsoleWindow.SetIcon(32, 32, Window.DefaultIconPixels);
            ConsoleWindow.Closed += new EventHandler(ConsoleWindowClosed);
            ConsoleWindow.Resized += new EventHandler<SizeEventArgs>(ConsoleWindowResized);
            UpdateConsole();
            ConsoleEnabled = true;
        }
        public static void DisableConsole()
        {
            ConsoleWindow.Close();
            ConsoleWindow = null;
            ConsoleEnabled = false;

            if (Settings.ExitOnConsoleClose)
                Game.Exit();
        }
        public static void ClearConsole()
        {
            ConsoleMessages.Clear();
        }
        public static void WriteConsole(string message)
        {
            if (ConsoleEnabled)
            {
                ConsoleMessages.Add(new Text(message, ConsoleFont, 14));

                while (ConsoleMessages.Count > Math.Floor((float)ConsoleWindow.Size.Y / (float)ConsoleFontLineSpacing))
                    ConsoleMessages.RemoveAt(0);

                UpdateConsole();
            }
        }
        private static void UpdateConsole()
        {
            ConsoleWindow.Clear(Color.Black);

            for (int i = ConsoleMessages.Count - 1;  i >= 0; i--)
            {
                Text text = ConsoleMessages[i];
                text.Position = new Vector2f(0, i * ConsoleFontLineSpacing);
                text.Color = ConsoleTextColor;
                text.Style = Text.Styles.Bold;
                ConsoleWindow.Draw(text);
            }

            ConsoleWindow.Display();
        }
        internal static void DispatchConsoleEvents()
        {
            ConsoleWindow.DispatchEvents();
        }
    }
}
