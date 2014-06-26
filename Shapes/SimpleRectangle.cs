using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a simple rectangle that is axis-aligned and can't rotate.
	/// </summary>
	[Serializable]
	public struct SimpleRectangle : IShape
	{
		private Point _Position;
		private Point _Size;

		private bool PointsChanged;
		private Point[] Points;
		private static Point[] PerpAxes = new Point[] { new Point(0f, 1f), new Point(1f, 0f) }; //should not be possible
 
		/// <summary>
		/// Coordinates of the position of the rectangle.
		/// If this is changed Position2 will change with it (moving the rectangle).
		/// Use Position1 to change the topleft position without altering Position2.
		/// </summary>
		public Point Position
		{
			get { return _Position; }
			set
			{
				_Position = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// The size of the rectangle.
		/// </summary>
		public Point Size
		{
			get { return _Size; }
			set
			{
				_Size = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// Coordinates of the topleft point of the rectangle.
		/// If this is changed Position2 will remain, thus altering the Size.
		/// </summary>
		public Point Position1
		{
			get { return _Position; }
			set
			{
				_Size += _Position - value;
				_Position = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// Coordinates of the bottomright point of the rectangle.
		/// X + Width &amp; Y + Height.
		/// </summary>
		public Point Position2
		{
			get { return _Position + _Size; }
			set 
			{ 
				_Size = value - _Position;
				PointsChanged = true;
			}
		}

		///// <summary>
		///// Coordinates of the exact center of the rectangle.
		///// </summary>
		//public Point Center
		//{
		//	get { return _Position + _Size / 2f; }
		//}

		/// <summary>
		/// Does nothing, rotation of a simplerectangle is not possible.
		/// </summary>
		public float Rotation
		{
			get { return 0f; }
			set { }
		}

		public SimpleRectangle(Point position, Point size)
		{
			_Position = position;
			_Size = size;

			PointsChanged = true;
			Points = new Point[4];
		}
		public SimpleRectangle(float x, float y, float width, float height)
			: this(new Point(x, y), new Point(width, height)) { }

		public Point[] GetPoints()
		{
			if (PointsChanged)
				UpdatePoints();

			return Points;
		}

		public Point[] GetPerpAxes()
		{
			return PerpAxes;
		}

		private void UpdatePoints()
		{
			Points[0] = _Position;
			Points[1] = _Position + new Point(_Size.X, 0f);
			Points[2] = _Position + _Size;
			Points[3] = _Position + new Point(0f, _Size.Y);

			PointsChanged = false;
		}

		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(this, shape);
		}

		/// <summary>
		/// Why? =(
		/// </summary>
		public SimpleRectangle GetEnclosingSimpleRectangle()
		{
			return this;
		}

		/// <summary>
		/// Returns the 4 lines representing the sides of this rectangle.
		/// </summary>
		public Line[] GetBoundLines()
		{
			if (PointsChanged)
				UpdatePoints();

			return new Line[]
			{
				new Line(Points[0], Points[1]),
				new Line(Points[1], Points[2]),
				new Line(Points[2], Points[3]),
				new Line(Points[3], Points[0])
			};
		}

		public static bool operator ==(SimpleRectangle rect1, SimpleRectangle rect2)
		{
			return (rect1._Position == rect2._Position && rect1._Size == rect2._Size);
		}
		public static bool operator !=(SimpleRectangle rect1, SimpleRectangle rect2)
		{
			return (rect1._Position != rect2._Position || rect1._Size != rect2._Size);
		}
		public override bool Equals(object obj)
		{
			if (!(obj is SimpleRectangle))
				return false;

			SimpleRectangle rect = (SimpleRectangle)obj;
			return this == rect;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static SimpleRectangle operator +(SimpleRectangle rect, Point point)
		{
			return new SimpleRectangle(rect._Position + point, rect._Size);
		}
		public static SimpleRectangle operator -(SimpleRectangle rect, Point point)
		{
			return new SimpleRectangle(rect._Position - point, rect._Size);
		}

		public static SimpleRectangle operator *(SimpleRectangle rect, Point point)
		{
			return new SimpleRectangle(rect._Position, rect._Size * point);
		}
		public static SimpleRectangle operator /(SimpleRectangle rect, Point point)
		{
			return new SimpleRectangle(rect._Position, rect._Size / point);
		}

		public static explicit operator SDL2.SDL.SDL_Rect(SimpleRectangle rect)
		{
			return new SDL2.SDL.SDL_Rect { x = (int)rect._Position.X, y = (int)rect._Position.Y, w = (int)rect._Size.X, h = (int)rect._Size.Y };
		}

		public override string ToString()
		{
			return String.Format("({0}, {1})", _Position, _Size);
		}

		public static readonly SimpleRectangle Zero = new SimpleRectangle(Point.Zero, Point.Zero);
	}
}