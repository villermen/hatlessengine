using System;

namespace HatlessEngine
{
	/// <summary>
	/// 2 points, connected, line!
	/// </summary>
	public struct Line
	{
		public float X1;
		public float Y1;
		public float X2;
		public float Y2;
		public float Width;

		public Point Point1
		{
			get { return new Point(X1, Y1); }
			set { X1 = value.X;	Y1 = value.Y; }
		}
		public Point Point2
		{
			get { return new Point(X2, Y2); }
			set { X2 = value.X;	Y2 = value.Y; }
		}

		public float Length
		{
			get { return Point1.DistanceTo(Point2); }
		}

		public Line(float x1, float y1, float x2, float y2, float width = 1f)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
			Width = width;
		}
		public Line(Point pos1, Point pos2, float width = 1f)
			: this(pos1.X, pos1.Y, pos2.X, pos2.Y, width) { }

		public static explicit operator Rectangle(Line line)
		{
			//calculate upperleft point of rectangle
			//(-y, x) left normalized perp axis
			float length = line.Length;
			Point perp = new Point((-line.Y2 + line.Y1) / length, (line.X2 - line.X1) / length);
			Point rectPosition = new Point(line.X1 + perp.X * length * 0.5f, line.Y1 + perp.Y * length * 0.5f);
			float rectRotation = -rectPosition.AngleTo(line.Point1);

			//can be cast to rectangle for collision detection
			return new Rectangle(rectPosition, new Point(line.Width, length), rectRotation);
		}
	}
}

