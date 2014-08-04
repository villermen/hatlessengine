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
			Area = rect;

			Color = color;
		}
	}
}
