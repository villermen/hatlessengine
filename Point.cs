using System;

namespace HatlessEngine
{
	public struct Point : IShape
	{
		public float X;
		public float Y;

		public Point Position
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		public Point[] Points
		{
			get { return new Point[] { this }; }
		}

		public Point[] PerpAxes
		{
			get { return new Point[0]; }
		}

		public float Length
		{
			get { return DistanceTo(Point.Zero); }
		}
		public float Angle
		{
			get { return Point.Zero.AngleTo(this); }
		}

		public Point(float x, float y)
		{
			X = x;
			Y = y;
		}

		public float DistanceTo(Point point)
		{
			return (float)Math.Sqrt(Math.Pow(point.X - X, 2) + Math.Pow(point.Y - Y, 2));  
		}

		public float AngleTo(Point point)
		{
			float result = (float)(Math.Atan2(point.X - X, Y - point.Y) / Math.PI * 180);
			if (result < 0)
			{
				result += 360;
			}
			return result;
		}

		/// <summary>
		/// Rotates this point over an absolute origin by [angle] degrees.
		/// </summary>
		/// <param name="origin">Absolute origin.</param>
		/// <param name="angle">Degrees to rotate.</param>
		public void RotateOverOrigin(Point origin, float angle)
		{
			//convert angle to radians
			angle = (float)(Math.PI / 180 * angle);

			float T; //dont use already updated X for calculating Y
			T = (float)(origin.X + (X - origin.X) * Math.Cos(angle) - (Y - origin.Y) * Math.Sin(angle));
			Y = (float)(origin.Y + (X - origin.X) * Math.Sin(angle) + (Y - origin.Y) * Math.Cos(angle));
			X = T;

		}

		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(this, shape);
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

		public static explicit operator System.Drawing.PointF(Point point)
		{
			return new System.Drawing.PointF(point.X, point.Y);
		}
		public static implicit operator Point(float f)
		{
			return new Point(f, f);
		}

		public override string ToString()
		{
			return "(" + X.ToString() + ", " + Y.ToString() + ")";
		}

		/// <summary>
		/// Point with X = 0 and Y = 0. Origin if you will.
		/// </summary>
		public static readonly Point Zero = new Point(0, 0);
	}
}

