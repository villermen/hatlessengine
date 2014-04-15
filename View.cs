using System;

namespace HatlessEngine
{
	[Serializable]
	public struct View
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
