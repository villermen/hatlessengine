using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a rectangle that can be rotated over an origin.
	/// </summary>
	[Serializable]
	public class ComplexRectangle : Shape
	{
		private Point _Origin;

		/// <summary>
		/// Origin is the offset from the topleft-most corner to the position before rotation is applied.
		/// </summary>
		public Point Origin
		{
			get { return _Origin; }
			set 
			{ 
				_Origin = value;
				Changed = true;
			}
		}

		public ComplexRectangle(Point pos, Point size, Point origin, float rotation = 0)
		{
			_Position = pos;
			_Size = size;
			_Origin = origin;
			_Rotation = rotation;

			Points = new Point[4];
			PerpAxes = new Point[4];
			BoundLines = new Line[4];
		}
		public ComplexRectangle(Point position, Point size, float rotation = 0)
			: this(position, size, Point.Zero, rotation) { }
		public ComplexRectangle(float x, float y, float width, float height, float originX, float originY, float rotation = 0)
			: this(new Point(x, y), new Point(width, height), new Point(originX, originY), rotation) { }
		public ComplexRectangle(float x, float y, float width, float height, float rotation = 0)
			: this(new Point(x, y), new Point(width, height), Point.Zero, rotation) { }
		public ComplexRectangle(ComplexRectangle rect)
			: this(rect._Position, rect._Size, rect._Origin, rect._Rotation) { }
		public ComplexRectangle()
			: this(Zero) { }

		protected override void Recalculate()
		{
			Points[0] = new Point(_Position.X - _Origin.X, _Position.Y - _Origin.Y).RotateOverOrigin(_Position, _Rotation);
			Points[1] = new Point(_Position.X - _Origin.X + _Size.X, _Position.Y - _Origin.Y).RotateOverOrigin(_Position, _Rotation);
			Points[2] = new Point(_Position.X - _Origin.X + _Size.X, _Position.Y - _Origin.Y + _Size.Y).RotateOverOrigin(_Position, _Rotation);
			Points[3] = new Point(_Position.X - _Origin.X, _Position.Y - _Origin.Y + _Size.Y).RotateOverOrigin(_Position, _Rotation);

			PerpAxes[0] = new Point((-Points[1].Y + Points[0].Y) / _Size.X, (Points[1].X - Points[0].X) / _Size.X);
			PerpAxes[1] = new Point((-Points[2].Y + Points[1].Y) / _Size.Y, (Points[2].X - Points[1].X) / _Size.Y);
			PerpAxes[2] = PerpAxes[0];
			PerpAxes[3] = PerpAxes[1];

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

			BoundLines =  new Line[4]
			{
				new Line(Points[0], Points[1]),
				new Line(Points[1], Points[2]),
				new Line(Points[2], Points[3]),
				new Line(Points[3], Points[0])
			};

			Changed = false;
		}

		public static readonly ComplexRectangle Zero = new ComplexRectangle(Point.Zero, Point.Zero, Point.Zero, 0f);

		/// <summary>
		/// Cast to a rectangle, ignoring the rotation.
		/// Will only create the exact same shape if Rotation is 0f.
		/// </summary>
		public static explicit operator Rectangle(ComplexRectangle cRect)
		{
			return new Rectangle(cRect._Position - cRect._Origin, cRect._Size);
		}
	}
}