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

		private bool Changed;
		private Point[] Points;
		private Point[] PerpAxes;
		private Rectangle EnclosingRectangle;

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
				Changed = true;
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
				Changed = true;
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
				Changed = true;
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
				Changed = true;
			}
		}

		public Line(Point point1, Point point2)
		{
			_Point1 = point1;
			_Point2 = point2;

			Changed = true;
			Points = new Point[2];
			PerpAxes = new Point[1];
			EnclosingRectangle = Rectangle.Zero;
		}
		public Line(float x1, float y1, float x2, float y2)
			: this(new Point(x1, y1), new Point(x2, y2)) { }

		public Point[] GetPoints()
		{
			if (Changed)
				Recalculate();

			return Points;
		}

		public Point[] GetPerpAxes()
		{
			if (Changed)
				Recalculate();

			return PerpAxes;
		}

		public Rectangle GetEnclosingRectangle()
		{
			if (Changed)
				Recalculate();

			return EnclosingRectangle;
		}

		private void Recalculate()
		{
			Points[0] = _Point1;
			Points[1] = _Point2;

			PerpAxes[0] = new Point((-_Point2.Y + _Point1.Y) / Length, (_Point2.X - _Point1.X) / Length);

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
			EnclosingRectangle = new Rectangle(minX, minY, maxX - minX, maxY - minY);

			Changed = false;
		}

		/// <summary>
		/// Creates a complex rectangle of this line by applying a width to it.
		/// </summary>
		public ComplexRectangle ToRectangle(float width)
		{
			return new ComplexRectangle(Position, new Point(width, Length), new Point(width / 2, Length), _Point1.GetAngleTo(_Point2));
		}

		public override string ToString()
		{
			return String.Format("({0}, {1})", Point1, Point2);
		}
	}
}