using System;

namespace HatlessEngine
{
    public class View
    {
		public Rectangle Area;
		public Rectangle Viewport;

		internal View(Rectangle area, Rectangle viewport)
        {
            Area = area;
            Viewport = viewport;
        }
    }
}
