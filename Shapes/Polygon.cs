using System;

namespace HatlessEngine
{
	/// <summary>
	/// A polygon where the last point connects back to the first one.
	/// Can be made to represent any CONVEX shape not covered.
	/// What is that? A Circle? Sorry, never heard of it.
	/// </summary>
	[Serializable]
	public class Polygon : Shape
	{
		private readonly Point[] _basePoints;

		/// <summary>
		/// Position, and points relative to this position.
		/// The polygon will be checked for convexness and throw an exception if it's not.
		/// </summary>
		public Polygon(Point pos, params Point[] points)
		{
			_Position = pos;

			//check for convexness
			float previousAngle = points[points.Length - 1].GetAngleTo(points[0]);
			float currentAngle  = points[0].GetAngleTo(points[1]);
			bool clockwise = Misc.GetRelativeAngle(previousAngle, currentAngle) >= 0f;

			previousAngle = currentAngle;

			for (int i = 1; i < points.Length - 1; i++)
			{
				currentAngle = points[i].GetAngleTo(points[i + 1]);

				if ((clockwise && Misc.GetRelativeAngle(previousAngle, currentAngle) < 0f) 
					|| (!clockwise && Misc.GetRelativeAngle(previousAngle, currentAngle) > 0f))
					throw new NonConvexShapeDesignException();

				previousAngle = currentAngle;
			}

			_basePoints = points;

			//initialize arrays
			Points = new Point[points.Length];
			PerpAxes = new Point[points.Length];
			BoundLines = new Line[points.Length];
		}

		protected override void Recalculate()
		{
			float minX = float.PositiveInfinity, minY = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
			
			for (int i = 0; i < _basePoints.Length; i++)
			{
				Points[i] = _Position + _basePoints[i].RotateOverOrigin(Point.Zero, _Rotation) * _Size;

				if (Points[i].X < minX)
					minX = Points[i].X;
				if (Points[i].Y < minY)
					minY = Points[i].Y;
				if (Points[i].X > maxX)
					maxX = Points[i].X;
				if (Points[i].Y > maxY)
					maxY = Points[i].Y;

				if (i < _basePoints.Length - 1)
					BoundLines[i] = new Line(Points[i], Points[i + 1]);
				else
					BoundLines[i] = new Line(Points[i], Points[0]);

				PerpAxes[i] = BoundLines[i].GetPerpAxes()[0];
			}

			EnclosingRectangle = new Rectangle(minX, minY, maxX - minX, maxY - minY);

			Changed = false;
		}
	}
}