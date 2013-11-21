using System;
using SFML.Window;

namespace HatlessEngine
{
    public class View
    {
        internal SFML.Graphics.View SFMLView;

        public string Id { get; private set; }

        public Rectangle Area;
        public Window TargetWindow;
        public Rectangle Viewport;

        internal View(string id, Rectangle area, string targetWindow, Rectangle viewport)
        {
            Area = area;
            TargetWindow = Resources.Windows[targetWindow];
            Viewport = viewport;

            SFMLView = new SFML.Graphics.View(area);
            SFMLView.Viewport = viewport;

            TargetWindow.ActiveViews.Add(this);
        }

        internal void UpdateSFMLView()
        {
            SFMLView.Reset(Area);
            SFMLView.Viewport = Viewport;
        }
    }
}
