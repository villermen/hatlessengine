using System;

namespace HatlessEngine
{
	/// <summary>
	/// 2 points, connected, line!
	/// </summary>
	[Serializable]
	public class Line : Shape
	{
		/// <summary>
		/// Gets Position, if set will change Position without altering Point2 (unlike Position, which will move both).
		/// </summary>
		public Point Point1
		{
			get { return _Position; }
			set
			{
				_Size += Position - value;
				_Position = value;
				Changed = true;
			}
		}

		/// <summary>
		/// The point diagonal of the Position/Point1.
		/// </summary>
		public Point Point2
		{
			get { return _Position + _Size; }
			set
			{
				_Size = value - _Position;
				Changed = true;
			}
		}

		/// <summary>
		/// Gets the distance between both points.
		/// Moves Point2 to obtain the given length if set.
		/// </summary>
		public float Length
		{
			get { return Size.GetDistanceFromOrigin(); }
			set
			{
				//multiply size by the difference factor
				_Size *= (value / Length);
				Changed = true;
			}
		}

		public Line(Point point1, Point point2)
		{
			_Position = point1;
			Point2 = point2;

			Points = new Point[2];
			PerpAxes = new Point[1];
		}
		public Line(float x1, float y1, float x2, float y2)
			: this(new Point(x1, y1), new Point(x2, y2)) { }

		protected override void Recalculate()
		{
			Points[0] = _Position;
			Points[1] = _Position + _Size;

			PerpAxes[0] = new Point((-Point2.Y + Point1.Y) / Length, (Point2.X - Point1.X) / Length);

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
		public ComplexRectangle ToComplexRectangle(float width)
		{
			return new ComplexRectangle(_Position, new Point(width, Length), new Point(width / 2, Length), Point1.GetAngleTo(Point2));
		}
	}
}