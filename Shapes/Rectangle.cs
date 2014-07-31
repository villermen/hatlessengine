using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a simple axis-aligned rectangle.
	/// </summary>
	[Serializable]
	public class Rectangle : Shape
	{
		public new float Rotation { get; set; }

		/// <summary>
		/// Coordinates of the point diagonal of the position of the rectangle (bottomright if Size is positive on both axes).
		/// Basically X + Width &amp; Y + Height.
		/// Cannot be changed directly, use Size and Position for that.
		/// </summary>
		public Point Position2
		{
			get { return _Position + _Size; }
		}

		/// <summary>
		/// Coordinates of the exact center of the rectangle.
		/// </summary>
		public Point Center
		{
			get { return _Position + _Size / 2f; }
		}

		public Rectangle(Point pos, Point size)
		{
			_Position = pos;
			_Size = size;

			Points = new Point[4];
			PerpAxes = new Point[] { new Point(0f, 1f), new Point(1f, 0f), new Point(0f, 1f), new Point(1f, 0f) };
		}
		public Rectangle(float x, float y, float width, float height)
			: this(new Point(x, y), new Point(width, height)) { }

		protected override void Recalculate()
		{
			Points[0] = _Position;
			Points[1] = _Position + new Point(_Size.X, 0f);
			Points[2] = _Position + _Size;
			Points[3] = _Position + new Point(0f, _Size.Y);

			BoundLines = new Line[] {
				new Line(Points[0], Points[1]),
				new Line(Points[1], Points[2]),
				new Line(Points[2], Points[3]),
				new Line(Points[3], Points[0]),
			};

			Changed = false;
		}

		public static bool operator ==(Rectangle rect1, Rectangle rect2)
		{
			return (rect1._Position == rect2._Position && rect1._Size == rect2._Size);
		}
		public static bool operator !=(Rectangle rect1, Rectangle rect2)
		{
			return (rect1._Position != rect2._Position || rect1._Size != rect2._Size);
		}
		public override bool Equals(object obj)
		{
			if (!(obj is Rectangle))
				return false;

			Rectangle rect = (Rectangle)obj;
			return this == rect;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static Rectangle operator +(Rectangle rect, Point point)
		{
			return new Rectangle(rect._Position + point, rect._Size);
		}
		public static Rectangle operator -(Rectangle rect, Point point)
		{
			return new Rectangle(rect._Position - point, rect._Size);
		}

		public static Rectangle operator *(Rectangle rect, Point point)
		{
			return new Rectangle(rect._Position, rect._Size * point);
		}
		public static Rectangle operator /(Rectangle rect, Point point)
		{
			return new Rectangle(rect._Position, rect._Size / point);
		}

		public static explicit operator SDL2.SDL.Rect(Rectangle rect)
		{
			return new SDL2.SDL.Rect { x = (int)rect._Position.X, y = (int)rect._Position.Y, w = (int)rect._Size.X, h = (int)rect._Size.Y };
		}

		public override string ToString()
		{
			return String.Format("({0}, {1})", _Position, _Size);
		}

		public static readonly Rectangle Zero = new Rectangle(Point.Zero, Point.Zero);

		public static explicit operator ComplexRectangle(Rectangle rect)
		{
			return new ComplexRectangle(rect._Position, rect._Size);
		}
	}
}