using System;
using SDL2;

namespace HatlessEngine
{
	internal struct LineDrawJob : IDrawJob
	{
		public DrawJobType Type { get; set; }
		public int Depth { get; set; }
		public Rectangle Area { get; set; }

		public Point[] Points;
		public int PointCount;
		public Color Color;

		/// <summary>
		/// Line(s) job.
		/// </summary>
		public LineDrawJob(int depth, Rectangle area, Point[] points, Color color)
			: this()
		{
			PointCount = points.Length;
			Points = points;
			Color = color;

			Type = DrawJobType.Lines;
			Depth = depth;
			Area = area;
		}
	}
}