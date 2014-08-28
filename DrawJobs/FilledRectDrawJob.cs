using System;

namespace HatlessEngine
{
	internal struct FilledRectDrawJob : IDrawJob
	{
		public int Depth { get; set; }
		public Rectangle Area { get; set; }

		public Color Color;

		public FilledRectDrawJob(int depth, Rectangle rect, Color color)
			: this()
		{
			Depth = depth;
			Area = new Rectangle(rect);
			//add 1 to size because sdl's filled rects dont fill including the last pixel
			Area.Size += 1f;

			Color = color;
		}
	}
}
