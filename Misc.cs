using System;
using System.Drawing;
using System.Collections.Generic;

namespace HatlessEngine
{
    public static class Misc
    {
        /// <summary>
        /// Returns whether a random chance is met.
        /// Explained: chance in values (1 in 100) chance of returning true
        /// </summary>
        /// <returns>Whether chance is met</returns>
        public static bool Chance(int values, int chance = 1)
        {
            int result = new Random().Next(values);
            if (result < chance)
                return true;
            else
                return false;
        }

		public static bool ShapesIntersecting(IShape shape1, IShape shape2)
		{
			Point[] shape1Points = shape1.Points;
			Point[] shape2Points = shape2.Points;
			List<Point> allAxes = new List<Point>(shape1.Axes);
			allAxes.AddRange(shape2.Axes);

			foreach(Point axis in allAxes)
			{
				float thisScalarMin = float.PositiveInfinity;
				float thisScalarMax = float.NegativeInfinity;
				float otherScalarMin = float.PositiveInfinity;
				float otherScalarMax = float.NegativeInfinity;

				foreach(Point point in shape1Points)
				{
					float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < thisScalarMin)
						thisScalarMin = scalar;
					if (scalar > thisScalarMax)
						thisScalarMax = scalar;				
				}
				foreach(Point point in shape2Points)
				{
					float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < otherScalarMin)
						otherScalarMin = scalar;
					if (scalar > otherScalarMax)
						otherScalarMax = scalar;		
				}

				//something does not overlap -> not intersecting
				if (thisScalarMax < otherScalarMin || thisScalarMin > otherScalarMax)
					return false;
			}
			//everything´s overlapping -> intersecting
			return true;
		}

		[Obsolete("Not implemented yet.")]
		public static bool ShapesIntersectingWithSpeed(IShape shape1, IShape shape2, Point relativeSpeed, out float touchingAtSpeedFraction)
		{
			touchingAtSpeedFraction = 0f;
			return false;
		}
		[Obsolete("Not implemented yet.")]
		public static bool ShapesIntersectingWithSpeed(IShape shape1, IShape shape2, Point relativeSpeed)
		{
			float touchingAtSpeedFraction;
			return ShapesIntersectingWithSpeed(shape1, shape2, relativeSpeed, out touchingAtSpeedFraction);
		}
		[Obsolete("Not implemented yet.")]
		public static bool ShapesIntersectingWithSpeed(IShape shape1, IShape shape2, Point shape1Speed, Point shape2Speed, out float touchingAtSpeedFraction)
		{
			return ShapesIntersectingWithSpeed(shape1, shape2, shape1Speed - shape2Speed, out touchingAtSpeedFraction);
		}
		[Obsolete("Not implemented yet.")]
		public static bool ShapesIntersectingWithSpeed(IShape shape1, IShape shape2, Point shape1Speed, Point shape2Speed)
		{
			float touchingAtSpeedFraction;
			return ShapesIntersectingWithSpeed(shape1, shape2, shape1Speed - shape2Speed, out touchingAtSpeedFraction);
		}

		/*public bool IntersectsWith(Shape shape)
		{
			var pnt = (shape.GetType())shape;

			Point[] thisPoints = Points;
			Point[] otherPoints = shape.Points;
			List<Point> allAxes = new List<Point>(Axes);
			allAxes.AddRange(shape.Axes);

			foreach(Point axis in allAxes)
			{
				float thisScalarMin = float.PositiveInfinity;
				float thisScalarMax = float.NegativeInfinity;
				float otherScalarMin = float.PositiveInfinity;
				float otherScalarMax = float.NegativeInfinity;

				foreach(Point point in thisPoints)
				{
					float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < thisScalarMin)
						thisScalarMin = scalar;
					if (scalar > thisScalarMax)
						thisScalarMax = scalar;				
				}
				foreach(Point point in otherPoints)
				{
					float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < otherScalarMin)
						otherScalarMin = scalar;
					if (scalar > otherScalarMax)
						otherScalarMax = scalar;		
				}

				//something does not overlap -> not intersecting
				if (thisScalarMax < otherScalarMin || thisScalarMin > otherScalarMax)
					return false;
			}
			//everything´s overlapping -> intersecting
			return true;
		}
		//		public bool WillCollideWith(Shape shape, Point thisSpeed, Point otherSpeed)
		//		{
		//		}
		//		public bool WillCollideWith(Shape shape, Point thisSpeed, Point otherSpeed, out float collisionAtSpeedFraction)
		//		{
		//		}


		public bool IntersectsWith(Rectangle rectangle)
		{
			//load in all points of the rectangles for efficiency
			Point[] points = {Point1, Point2, Point3, Point4, rectangle.Point1, rectangle.Point2, rectangle.Point3, rectangle.Point4};

			//calculate 2 perpendicular axes for both rectangles (so 4 total) (-Y, X)
			Point[] axes = new Point[4];
			axes[0].X = -points[1].Y + points[0].Y;
			axes[0].Y = points[1].X - points[0].X;
			axes[1].X = -points[2].Y + points[1].Y;
			axes[1].Y = points[2].X - points[1].X;
			axes[2].X = -points[5].Y + points[4].Y;
			axes[2].Y = points[5].X - points[4].X;
			axes[3].X = -points[6].Y + points[5].Y;
			axes[3].Y = points[6].X - points[5].X;

			//project all points onto axes till a non-overlap has been detected by using scalars
			for (byte axis = 0; axis < 4; axis++)
			{
				float thisScalarMin = 0, thisScalarMax = 0, otherScalarMin = 0, otherScalarMax = 0;

				for(byte point = 0; point < 8; point++)
				{
					float multiplier = (float)((points[point].X * axes[axis].X + points[point].Y * axes[axis].Y) / (Math.Pow(axes[axis].X, 2) + Math.Pow(axes[axis].Y, 2)));
					float scalar = multiplier * axes[axis].X * axes[axis].X + multiplier * axes[axis].Y * axes[axis].Y;
				
					if (point == 0)
					{
						thisScalarMin = scalar;
						thisScalarMax = scalar;
					}
					else if (point < 4)
					{
						thisScalarMin = Math.Min(scalar, thisScalarMin);
						thisScalarMax = Math.Max(scalar, thisScalarMax);
					}
					else if (point == 4)
					{
						otherScalarMin = scalar;
						otherScalarMax = scalar;
					}
					else
					{
						otherScalarMin = Math.Min(scalar, otherScalarMin);
						otherScalarMax = Math.Max(scalar, otherScalarMax);
					}
				}

				//one projection does not overlap -> no intersection
				if (thisScalarMax < otherScalarMin || thisScalarMin > otherScalarMax)
					return false;
			}

			//all projections overlapped -> intersection
			return true;
		}
		/// <summary>
		/// Checks against a point (only projects on the 2 axes of this rectangle.
		/// </summary>
		public bool IntersectsWith(Point point)
		{
			Point[] points = { Point1, Point2, Point3, Point4, point };
			Point[] axes = new Point[2];
			axes[0].X = -points[1].Y + points[0].Y;
			axes[0].Y = points[1].X - points[0].X;
			axes[1].X = -points[2].Y + points[1].Y;
			axes[1].Y = points[2].X - points[1].X;

			for(byte axis = 0; axis < 2; axis++)
			{
				float thisScalarMin = 0, thisScalarMax = 0, otherScalar = 0;

				for(byte iPoint = 0; iPoint < 5; iPoint++)
				{
					float multiplier = (float)((points[iPoint].X * axes[axis].X + points[iPoint].Y * axes[axis].Y) / (Math.Pow(axes[axis].X, 2) + Math.Pow(axes[axis].Y, 2)));
					float scalar = multiplier * axes[axis].X * axes[axis].X + multiplier * axes[axis].Y * axes[axis].Y;

					if (iPoint == 0)
					{
						thisScalarMin = scalar;
						thisScalarMax = scalar;
					}
					else if (iPoint < 4)
					{
						thisScalarMin = Math.Min(scalar, thisScalarMin);
						thisScalarMax = Math.Max(scalar, thisScalarMax);
					}
					else
						otherScalar = scalar;
				}

				//one projection does not overlap -> no intersection
				if (thisScalarMax < otherScalar || thisScalarMin > otherScalar)
					return false;
			}

			return true;
		}*/
    }
}
