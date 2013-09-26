using System;

namespace HatlessEngine 
{
    public class Window
    {
        public string Id { get; private set; }
        internal SFML.Graphics.RenderWindow SFMLWindow;
        internal static byte[] DefaultIconPixels;

        static Window()
        {
            DefaultIconPixels = new SFML.Graphics.Image(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("HatlessEngine.defaultwindowicon.png")).Pixels;
        }

        public Window(string id, uint width, uint height, string title)
        {
            Id = id;
            SFMLWindow = new SFML.Graphics.RenderWindow(new SFML.Window.VideoMode(width, height), title);
            SFMLWindow.GainedFocus += new EventHandler(SFMLWindowGainedFocus);
            SFMLWindow.LostFocus += new EventHandler(SFMLWindowLostFocus);
            SFMLWindow.Closed += new EventHandler(SFMLWindowClosed);
            SFMLWindowGainedFocus(this, EventArgs.Empty);

            SFMLWindow.SetIcon(32, 32, DefaultIconPixels);
        }        

        /// <summary>
        /// Sends wrapper event from this window
        /// </summary>
        private void SFMLWindowGainedFocus(object sender, EventArgs e)
        {
            Game.FocusedWindow = Id;
        }

        private void SFMLWindowLostFocus(object sender, EventArgs e)
        {
            Game.FocusedWindow = "";
        }

        private void SFMLWindowClosed(object sender, EventArgs e)
        {
            Game.RemoveWindows.Add(Id);
        }
    }
}
