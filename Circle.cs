using System;

namespace HatlessEngine
{
	public struct Circle
	{
		public float X;
		public float Y;
		public float Radius;

		public Point Position
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		public Circle(float x, float y, float radius)
		{
			X = x;
			Y = y;
			Radius = radius;
		}
		public Circle(Point point, float radius)
			: this(point.X, point.Y, radius) { }
	}
}

