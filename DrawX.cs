using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Static class containing settings used in Draw methods.
	/// Also provides centralized access to draw functions and debug shape-drawing methods.
	/// </summary>
	public static class DrawX
	{
		internal static List<IDrawJob> DrawJobs = new List<IDrawJob>();

		public static Color BackgroundColor = Color.Gray;

		public static void Draw(Point point, Color color, int depth = 0)
		{
			DrawJobs.Add(new LineDrawJob(depth, new Rectangle(point, Point.Zero), new Point[] { point, point }, color));
		}
		public static void Draw(Line line, Color color, int depth = 0)
		{
			DrawJobs.Add(new LineDrawJob(depth, line.GetEnclosingRectangle(), line.GetPoints(), color));
		}
		public static void Draw(string str, Font font, Point pos, Color color, Alignment alignment = Alignment.Top | Alignment.Left, int depth = 0)
		{
			font.Draw(str, pos, color, alignment, depth);
		}
		public static void Draw(Sprite sprite, Point pos, Point scale, Point origin, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			sprite.Draw(pos, scale, origin, frameIndex, rotation, depth);
		}
		public static void Draw(Sprite sprite, Point pos, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			sprite.Draw(pos, frameIndex, rotation, depth);
		}

		/// <summary>
		/// Draws the outline of a shape. Drawing filled shapes is (currently?) only possible for AABB's. See DrawFilledRect.
		/// </summary>
		public static void DrawShapeOutline(Shape shape, Color color, int depth = 0)
		{
			//add the start point at the end of the array so it will draw the last line
			Point[] shapePoints = shape.GetPoints();
			Point[] points = new Point[shapePoints.Length + 1];
			shapePoints.CopyTo(points, 0);
			points[points.Length - 1] = points[0];

			DrawJobs.Add(new LineDrawJob(depth, shape.GetEnclosingRectangle(), points, color));
		}

		public static void DrawFilledRect(Rectangle rect, Color color, int depth = 0)
		{
			DrawJobs.Add(new FilledRectDrawJob(depth, rect, color));
		}

		internal static List<IDrawJob> GetDrawJobsByArea(Rectangle area)
		{
			return DrawJobs.FindAll(j => j.Area.IntersectsWith(area));
		}
	}
}