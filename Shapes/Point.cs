using System;

namespace HatlessEngine
{
	[Serializable]
	public struct Point : IShape
	{
		public float X;
		public float Y;

		public Point Position
		{
			get { return this; }
			set { this = value; }
		}
		public float Rotation
		{
			get { return 0f; }
			set { }
		}

		public Point(float x, float y)
		{
			X = x;
			Y = y;
		}

		public Point[] GetPoints()
		{
			return new Point[] { this };
		}

		public Point[] GetPerpAxes()
		{
			return new Point[0];
		}

		/// <summary>
		/// Will return a SimpleRectangle with this point as it's Position and no Size.
		/// </summary>
		public SimpleRectangle GetEnclosingRectangle()
		{
			return new SimpleRectangle(this, Point.Zero);
		}

		public float GetDistanceTo(Point point)
		{
			return (float)Math.Sqrt(Math.Pow(point.X - X, 2) + Math.Pow(point.Y - Y, 2));  
		}
		public float GetDistanceFromOrigin()
		{
			return GetDistanceTo(Point.Zero);
		}

		public float GetAngleTo(Point point)
		{
			float result = (float)(Math.Atan2(point.X - X, Y - point.Y) / Math.PI * 180);
			if (result < 0)
				result += 360;
			return result;
		}
		public float GetAngleFromOrigin()
		{
			return Point.Zero.GetAngleTo(this);
		}

		/// <summary>
		/// Rotates this point over an absolute origin by [angle] degrees.
		/// </summary>
		/// <param name="origin">Absolute origin.</param>
		/// <param name="angle">Degrees to rotate.</param>
		public Point RotateOverOrigin(Point origin, float angle)
		{
			//convert angle to radians
			angle = (float)(Math.PI / 180 * angle);

			float T; //dont use already updated X for calculating Y
			T = (float)(origin.X + (X - origin.X) * Math.Cos(angle) - (Y - origin.Y) * Math.Sin(angle));
			Y = (float)(origin.Y + (X - origin.X) * Math.Sin(angle) + (Y - origin.Y) * Math.Cos(angle));
			X = T;
			return this;
		}

		public static bool operator ==(Point point1, Point point2)
		{
			return (point1.X == point2.X && point1.Y == point2.Y);
		}
		public static bool operator !=(Point point1, Point point2)
		{
			return (point1.X != point2.X || point1.Y != point2.Y);
		}
		public override bool Equals(object obj)
		{
			if (!(obj is Point))
			{
				return false;
			}
			Point point = (Point)obj;
			return point.X == X && point.Y == Y && point.GetType().Equals(base.GetType());
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static Point operator +(Point point1, Point point2)
		{
			return new Point(point1.X + point2.X, point1.Y + point2.Y);
		}
		public static Point operator -(Point point1, Point point2)
		{
			return new Point(point1.X - point2.X, point1.Y - point2.Y);
		}
		public static Point operator *(Point point1, Point point2)
		{
			return new Point(point1.X * point2.X, point1.Y * point2.Y);
		}
		public static Point operator /(Point point1, Point point2)
		{
			return new Point(point1.X / point2.X, point1.Y / point2.Y);
		}

		public static Point operator -(Point point)
		{
			return new Point(-point.X, -point.Y);
		}

		public static implicit operator Point(float f)
		{
			return new Point(f, f);
		}


		public static explicit operator SDL2.SDL.Point(Point point)
		{
			return new SDL2.SDL.Point { x = (int)point.X, y = (int)point.Y };
		}

		public override string ToString()
		{
			return String.Format("({0}, {1})", X, Y);
		}

		/// <summary>
		/// Point with X = 0 and Y = 0. Origin if you will.
		/// </summary>
		public static readonly Point Zero = new Point(0f, 0f);
	}
}