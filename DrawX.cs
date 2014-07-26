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
			DrawJobs.Add(new LineDrawJob(depth, point.GetEnclosingRectangle(), new Point[] { point, point }, color));
		}
		public static void Draw(Line line, Color color, int depth = 0)
		{
			DrawJobs.Add(new LineDrawJob(depth, line.GetEnclosingRectangle(), line.GetPoints(), color));
		}
		public static void Draw(Rectangle rect, Color color, int depth = 0)
		{
			Point[] points = new Point[5];
			rect.GetPoints().CopyTo(points, 0);
			points[4] = points[0]; //add first one again so sdl will complete the rectangle

			DrawJobs.Add(new LineDrawJob(depth, rect, points, color));
		}
		public static void Draw(Polygon poly, Color color, int depth = 0)
		{
			//same as rect
			Point[] polyPoints = poly.GetPoints(); //get points first to get array size
			Point[] points = new Point[polyPoints.Length + 1];
			polyPoints.CopyTo(points, 0);	
			points[points.Length - 1] = points[0];

			DrawJobs.Add(new LineDrawJob(depth, poly.GetEnclosingRectangle(), points, color));
		}
		public static void Draw(string str, Font font, Point pos, Color color, Alignment alignment = Alignment.Top | Alignment.Left, int depth = 0)
		{
			font.Draw(str, pos, color, alignment, depth);
		}
		public static void Draw(string str, string fontID, Point pos, Color color, Alignment alignment = Alignment.Top | Alignment.Left, int depth = 0)
		{
			Resources.Fonts[fontID].Draw(str, pos, color, alignment, depth);
		}
		public static void Draw(Sprite sprite, Point pos, Point scale, Point origin, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			sprite.Draw(pos, scale, origin, frameIndex, rotation, depth);
		}
		public static void Draw(Sprite sprite, Point pos, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			sprite.Draw(pos, frameIndex, rotation, depth);
		}
		public static void Draw(string spriteID, Point pos, Point scale, Point origin, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Resources.Sprites[spriteID].Draw(pos, scale, origin, frameIndex, rotation, depth);
		}
		public static void Draw(string spriteID, Point pos, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Resources.Sprites[spriteID].Draw(pos, frameIndex, rotation, depth);
		}

		internal static List<IDrawJob> GetDrawJobsByArea(Rectangle area)
		{
			return DrawJobs.FindAll(j => j.Area.IntersectsWith(area));
		}
	}
}