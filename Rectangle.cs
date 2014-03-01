using System;

namespace HatlessEngine
{
	public struct Rectangle : IShape
	{
		public float X;
		public float Y;
		public float Width;
		public float Height;
		public float OriginX;
		public float OriginY;
		public float Rotation;

		public Point Position
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}
		public Point Size
		{
			get { return new Point(Width, Height); }
			set { Width = value.X; Height = value.Y; }
		}

		/// <summary>
		/// Origin is the offset from the topleft-most corner to the position.
		/// </summary>
		public Point Origin
		{
			get { return new Point(OriginX, OriginY); }
			set { OriginX = value.X; OriginY = value.Y; }
		}

		public Point Point1
		{
			get 
			{
				Point point = new Point(X - OriginX, Y - OriginY);
				point.RotateOverOrigin(Position, Rotation);
				return point;
			}
		}
		public Point Point2
		{
			get 
			{
				Point point = new Point(X - OriginX + Width, Y - OriginY);
				point.RotateOverOrigin(Position, Rotation);
				return point;
			}
		}
		public Point Point3
		{
			get 
			{
				Point point = new Point(X - OriginX + Width, Y - OriginY + Height);
				point.RotateOverOrigin(Position, Rotation);
				return point;
			}
		}
		public Point Point4
		{
			get 
			{
				Point point = new Point(X - OriginX, Y - OriginY + Height);
				point.RotateOverOrigin(Position, Rotation);
				return point;
			}
		}

		/// <summary>
		/// Ignores rotation or origin offset, just takes the X + Width.
		/// </summary>
		public float X2
		{
			get { return X + Width; }
		}
		/// <summary>
		/// Ignores rotation or origin offset, just takes the Y + Height.
		/// </summary>
		public float Y2
		{
			get { return Y + Height; }
		}

		/// <summary>
		/// Returns an array with the 4 points in the rectangle.
		/// Better than directly using Point#.X or Point#.Y (will do the maths twice).
		/// </summary>
		public Point[] Points
		{
			get { return new Point[] { Point1, Point2, Point3, Point4 }; }
		}

		public Line[] Lines
		{
			get 
			{ 
				Point[] points = Points;
				return new Line[] 
				{
					new Line(points[0], points[1]),
					new Line(points[1], points[2]),
					new Line(points[2], points[3]),
					new Line(points[3], points[0])
				}; 
			}
		}

		public Point[] PerpAxes
		{
			get 
			{
				Point[] points = Points;
				Point[] axes = new Point[2];
				axes[0] = new Point((-points[1].Y + points[0].Y) / Width, (points[1].X - points[0].X) / Width);
				axes[1] = new Point((-points[2].Y + points[1].Y) / Height, (points[2].X - points[1].X) / Height);
				return axes;
			}
		}

		public Rectangle(float x, float y, float width, float height, float originX, float originY, float rotation = 0)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			OriginX = originX;
			OriginY = originY;
			Rotation = rotation;
		}
		public Rectangle(Point position, Point size, Point origin, float rotation = 0)
			: this(position.X, position.Y, size.X, size.Y, origin.X, origin.Y, rotation) { }
		public Rectangle(float x, float y, float width, float height, float rotation = 0)
			: this(x, y, width, height, 0, 0, rotation) { }
		public Rectangle(Point position, Point size, float rotation = 0) 
			: this(position.X, position.Y, size.X, size.Y, 0, 0, rotation) { }

		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(this, shape);
		}

		public static explicit operator System.Drawing.RectangleF(Rectangle rect)
		{
			return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		}
		public static explicit operator System.Drawing.Rectangle(Rectangle rect)
		{
			return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2}, {3})", Point1, Point2, Point3, Point4);
		}

		public static readonly Rectangle Zero = new Rectangle(0, 0, 0, 0);
	}
}

