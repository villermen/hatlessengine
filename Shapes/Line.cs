using System;

namespace HatlessEngine
{
	/// <summary>
	/// 2 points, connected, line!
	/// </summary>
	[Serializable]
	public struct Line : IShape
	{
		private Point _Point1;
		private Point _Point2;

		private bool PointsChanged;
		private Point[] Points;
		private Point[] PerpAxes;

		/// <summary>
		/// Changes this line's position, setting Point1 to Position and moving Point2 with it equally.
		/// </summary>
		public Point Position
		{
			get { return _Point1; }
			set 
			{ 
				_Point2 = value + _Point2 - _Point1;
				_Point1 = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// First Point and Position of the line.
		/// Use this property to change it without affecting Point2.
		/// </summary>
		public Point Point1
		{
			get { return _Point1; }
			set
			{
				_Point1 = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// Second point of the line.
		/// </summary>
		public Point Point2
		{
			get { return _Point2; }
			set
			{
				_Point2 = value;
				PointsChanged = true;
			}
		}

		/// <summary>
		/// Angle from Point1 to Point2.
		/// </summary>
		public float Rotation
		{
			get { return _Point1.GetAngleTo(_Point2); }
			set 
			{
				_Point2 = _Point2.RotateOverOrigin(_Point1, value - Rotation);
				PointsChanged = true;
			}
		}

		/// <summary>
		/// Gets the distance between both points.
		/// Moves Point2 to obtain the given length with set.
		/// </summary>
		public float Length
		{
			get { return Point1.GetDistanceTo(Point2); }
			set
			{
				//multiply point2 by difference factor
				_Point2 = (_Point2 - _Point1) * (value / Length);
				PointsChanged = true;
			}
		}

		public Line(Point point1, Point point2)
		{
			_Point1 = point1;
			_Point2 = point2;

			PointsChanged = true;
			Points = new Point[2];
			PerpAxes = new Point[1];
		}
		public Line(float x1, float y1, float x2, float y2)
			: this(new Point(x1, y1), new Point(x2, y2)) { }

		public Point[] GetPoints()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();

			return Points;
		}

		public Point[] GetPerpAxes()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();

			return PerpAxes;
		}

		private void UpdatePointsAndPerpAxes()
		{
			Points[0] = _Point1;
			Points[1] = _Point2;

			PerpAxes[0] = new Point((-_Point2.Y + _Point1.Y) / Length, (_Point2.X - _Point1.X) / Length);

			PointsChanged = false;
		}
		
		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(this, shape);
		}

		public SimpleRectangle GetEnclosingSimpleRectangle()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();

			float minX = float.PositiveInfinity, minY = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;

			foreach (Point p in Points)
			{
				if (p.X < minX)
					minX = p.X;
				if (p.Y < minY)
					minY = p.Y;
				if (p.X > maxX)
					maxX = p.X;
				if (p.Y > maxY)
					maxY = p.Y;
			}

			return new SimpleRectangle(minX, minY, maxX, maxY);
		}

		/// <summary>
		/// Creates a complex rectangle of this line by applying a width to it.
		/// </summary>
		public Rectangle ToRectangle(float width)
		{
			return new Rectangle(Position, new Point(width, Length), new Point(width / 2, Length), Rotation);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1})", Point1, Point2);
		}
	}
}