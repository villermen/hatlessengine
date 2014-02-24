using System;

namespace HatlessEngine
{
	/// <summary>
	/// 2 points, connected, line!
	/// </summary>
	public struct Line : IShape
	{
		public float X1;
		public float Y1;
		public float X2;
		public float Y2;

		public Point Position
		{
			get { return Point1; }
			set 
			{ 
				Point2 = value + Point2 - Point1;
				Point1 = value;
			}
		}

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

		public Point[] Points
		{
			get { return new Point[] { Point1, Point2 }; }
		}

		public Point[] PerpAxes
		{
			get 
			{
				Point[] points = Points;
				Point[] axes = new Point[1];
				axes[0] = new Point((-points[1].Y + points[0].Y) / Length, (points[1].X - points[0].X) / Length);
				return axes;
			}
		}

		public Line(float x1, float y1, float x2, float y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}
		public Line(Point pos1, Point pos2)
			: this(pos1.X, pos1.Y, pos2.X, pos2.Y) { }

		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(this, shape);
		}

		public Rectangle ToRectangle(float width)
		{
			//can be cast to rectangle for collision detection
			return new Rectangle(Position, new Point(width, Length), new Point(width / 2, Length), Point1.AngleTo(Point2));
		}
	}
}

