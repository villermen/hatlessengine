using System;
using SFML.Window;

namespace HatlessEngine
{
    public class View
    {
        internal SFML.Graphics.View SFMLView;

        private string Id;
        private float x;
        private float y;
        private float width;
        private float height;
        private float windowX;
        private float windowY;
        private float windowWidth;
        private float windowHeight;
        public float X
        {
            get { return x; }
            set 
            {
                SFMLView.Move(new Vector2f(value - x, 0));
            }
        }
        public float Y
        {
            get { return y; }
            set
            {
                SFMLView.Move(new Vector2f(0, value - y));
            }
        }
        public float Width
        {
            get { return width; }
            set
            {
                SFMLView.Size = new Vector2f(width / 2, height / 2);
            }
        }
        public float Height
        {
            get { return height; }
            set
            {
                SFMLView.Size = new Vector2f(width / 2, height / 2);
            }
        }
        public Window TargetWindow { get; private set; } //add set code
        public float WindowX 
        {
            get { return windowX; }
            set
            {
                SFMLView.Viewport = new SFML.Graphics.FloatRect(value, windowY, windowWidth, windowHeight);
                windowX = value;
            }
        }
        public float WindowY
        {
            get { return windowY; }
            set
            {
                SFMLView.Viewport = new SFML.Graphics.FloatRect(windowX, value, windowWidth, windowHeight);
                windowY = value;
            }
        }
        public float WindowWidth
        {
            get { return windowWidth; }
            set
            {
                SFMLView.Viewport = new SFML.Graphics.FloatRect(windowX, windowY, value, windowHeight);
                windowWidth = value;
            }
        }
        public float WindowHeight
        {
            get { return windowHeight; }
            set
            {
                SFMLView.Viewport = new SFML.Graphics.FloatRect(windowX, windowY, windowWidth, value);
                windowHeight = value;
            }
        }

        public View(string id, float viewX, float viewY, float viewWidth, float viewHeight, Window targetWindow, float windowXFraction = 0, float windowYFraction = 0, float windowWidthFraction = 1, float windowHeightFraction = 1)
        {
            x = viewX;
            y = viewY;
            width = viewWidth;
            height = viewHeight;
            TargetWindow = targetWindow;
            windowX = windowXFraction;
            windowY = windowYFraction;
            windowWidth = windowWidthFraction;
            windowHeight = windowHeightFraction;

            SFMLView = new SFML.Graphics.View(new SFML.Graphics.FloatRect(x, y, width, height));
            SFMLView.Viewport = new SFML.Graphics.FloatRect(windowX, windowY, windowWidth, windowHeight);

            targetWindow.ActiveViews.Add(this);
        }
    }
}
