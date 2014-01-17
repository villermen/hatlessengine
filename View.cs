using System;
using System.Drawing;

namespace HatlessEngine
{
    public class View
    {
		public RectangleF Area;
		public RectangleF Viewport;

		internal View(RectangleF area, RectangleF viewport)
        {
            Area = area;
            Viewport = viewport;
        }
    }
}
