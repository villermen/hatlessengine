using System;

namespace HatlessEngine
{
	[Serializable]
	public struct View
	{
		public SimpleRectangle Area;
		public SimpleRectangle Viewport;

		internal View(SimpleRectangle area, SimpleRectangle viewport)
		{
			//validate if viewport is entirely within the window
			if (viewport.Position1.X < 0f || viewport.Position1.Y < 0f || viewport.Position2.X > 1f || viewport.Position2.Y > 1f)
				throw new ArgumentOutOfRangeException("viewport", "viewport lies (partially) outside of the window area. Use fractional values (0-1) to specify the part of the window it should project to.");

			Area = area;
			Viewport = viewport;
		}
	}
}