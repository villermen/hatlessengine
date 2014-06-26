using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a complex rectangle that can be rotated over an origin.
	/// </summary>
	[Serializable]
	public struct Rectangle : IShape
	{
		private Point _Position;
		private Point _Size;
		private Point _Origin;
		private float _Rotation;

		/// <summary>
		/// For checking whether Points and PerpAxes have to be updated on request.
		/// </summary>
		private bool PointsChanged;
		private Point[] Points;
		private Point[] PerpAxes;
 
		/// <summary>
		/// Rectangle's position.
		/// Don't read as 'topleft point' as it will not be if rotation or origin is nonzero.
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
		/// Origin is the offset from the topleft-most corner to the position before rotation is applied.
		/// </summary>
		public Point Origin
		{
			get { return _Origin; }
			set 
			{ 
				_Origin = value;
				PointsChanged = true;
			}
		}
		/// <summary>
		/// Rotation of the rectangle around it's origin point.
		/// </summary>
		public float Rotation
		{
			get { return _Rotation; }
			set 
			{ 
				_Rotation = value;
				PointsChanged = true;
			}
		}

		public Rectangle(Point position, Point size, Point origin, float rotation = 0)
		{
			_Position = position;
			_Size = size;
			_Origin = origin;
			_Rotation = rotation;

			PointsChanged = true;
			Points = new Point[4];
			PerpAxes = new Point[2];
		}
		public Rectangle(Point position, Point size, float rotation = 0)
			: this(position, size, Point.Zero, rotation) { }
		public Rectangle(float x, float y, float width, float height, float originX, float originY, float rotation = 0)
			: this(new Point(x, y), new Point(width, height), new Point(originX, originY), rotation) { }
		public Rectangle(float x, float y, float width, float height, float rotation = 0)
			: this(new Point(x, y), new Point(width, height), Point.Zero, rotation) { }

		/// <summary>
		/// Returns an array with the 4 points of the rectangle.
		/// </summary>
		public Point[] GetPoints()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();
			
			return Points;
		}
		/// <summary>
		/// Returns an array with the 2 perpendicular axes.
		/// </summary>
		public Point[] GetPerpAxes()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();
			
			return PerpAxes;
		}

		private void UpdatePointsAndPerpAxes()
		{
			Points[0] = new Point(_Position.X - _Origin.X, _Position.Y - _Origin.Y).RotateOverOrigin(_Position, _Rotation);
			Points[1] = new Point(_Position.X - _Origin.X + _Size.X, _Position.Y - _Origin.Y).RotateOverOrigin(_Position, _Rotation);
			Points[2] = new Point(_Position.X - _Origin.X + _Size.X, _Position.Y - _Origin.Y + _Size.Y).RotateOverOrigin(_Position, _Rotation);
			Points[3] = new Point(_Position.X - _Origin.X, _Position.Y - _Origin.Y + _Size.Y).RotateOverOrigin(_Position, _Rotation);

			PerpAxes[0] = new Point((-Points[1].Y + Points[0].Y) / _Size.X, (Points[1].X - Points[0].X) / _Size.X);
			PerpAxes[1] = new Point((-Points[2].Y + Points[1].Y) / _Size.Y, (Points[2].X - Points[1].X) / _Size.Y);

			PointsChanged = false;
		}

		/// <summary>
		/// Returns the 4 lines representing the sides of this rectangle.
		/// </summary>
		public Line[] GetBoundLines()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();

			return new Line[]
			{
				new Line(Points[0], Points[1]),
				new Line(Points[1], Points[2]),
				new Line(Points[2], Points[3]),
				new Line(Points[3], Points[0])
			};
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

		public override string ToString()
		{
			if (PointsChanged)
				UpdatePointsAndPerpAxes();

			return String.Format("({0}, {1}, {2}, {3})", Points[0], Points[1], Points[2], Points[3]);
		}

		public static readonly Rectangle Zero = new Rectangle(Point.Zero, Point.Zero, Point.Zero, 0f);
	}
}