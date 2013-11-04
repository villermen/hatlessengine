using System;
using System.Collections.Generic;

namespace HatlessEngine 
{
    public class Window
    {
        public string Id { get; private set; }
        internal SFML.Graphics.RenderWindow SFMLWindow;
        internal static byte[] DefaultIconPixels;

        public int X { get; private set; }
        public int Y { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public List<View> ActiveViews = new List<View>();

        internal float MouseXOnWindow = 0;
        internal float MouseYOnWindow = 0;

        static Window()
        {
            DefaultIconPixels = new SFML.Graphics.Image(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("HatlessEngine.windowicon.png")).Pixels;
        }

        public Window(string id, uint width, uint height, string title)
        {
            Id = id;
            SFMLWindow = new SFML.Graphics.RenderWindow(new SFML.Window.VideoMode(width, height), title);
            SFMLWindow.GainedFocus += new EventHandler(SFMLWindowGainedFocus);
            SFMLWindow.LostFocus += new EventHandler(SFMLWindowLostFocus);
            SFMLWindow.Closed += new EventHandler(SFMLWindowClosed);
            SFMLWindow.Resized += new EventHandler<SFML.Window.SizeEventArgs>(SFMLWindowResized);
            SFMLWindow.MouseMoved += new EventHandler<SFML.Window.MouseMoveEventArgs>(SFMLWindowMouseMoved);
            SFMLWindow.MouseButtonPressed += new EventHandler<SFML.Window.MouseButtonEventArgs>(SFMLWindowGainedFocus);
            SFMLWindowGainedFocus(this, EventArgs.Empty);

            SFMLWindow.SetIcon(32, 32, DefaultIconPixels);

            Width = width;
            Height = height;

            X = SFMLWindow.Position.X;
            Y = SFMLWindow.Position.Y;
        }        

        /// <summary>
        /// Sends wrapper event from this window
        /// </summary>
        private void SFMLWindowGainedFocus(object sender, EventArgs e)
        {
            Game.FocusedWindow = this;
        }

        private void SFMLWindowLostFocus(object sender, EventArgs e)
        {
            Game.FocusedWindow = null;
        }

        private void SFMLWindowClosed(object sender, EventArgs e)
        {
            SFMLWindowLostFocus(this, EventArgs.Empty);

            SFMLWindow.Close();
            //SFMLWindow.Dispose(); DOES NOT WORK CURRENTLY TODO
            Resources.RemoveWindows.Add(this);
        }

        private void SFMLWindowResized(object sender, SFML.Window.SizeEventArgs e)
        {
            Width = e.Width;
            Height = e.Height;
            X = SFMLWindow.Position.X;
            Y = SFMLWindow.Position.Y;
        }

        private void SFMLWindowMouseMoved(object sender, SFML.Window.MouseMoveEventArgs e)
        {
            MouseXOnWindow = (float)e.X / Width;
            MouseYOnWindow = (float)e.Y / Height;
        }
    }
}
