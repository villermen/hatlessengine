namespace HatlessEngine
{
	internal struct LineDrawJob : IDrawJob
	{
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

			Depth = depth;
			Area = area;
		}
	}
}