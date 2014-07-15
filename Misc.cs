using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	public static class Misc
	{
		/// <summary>
		/// Whether to show a messagebox when an unhandled exception is thrown.
		/// Crashlog will be written regardless of this setting.
		/// This is disabled by default when DEBUG is defined, but can be changed at will.
		/// </summary>
#if DEBUG
		public static bool ExceptionErrorMessageEnabled = false;
#else
		public static bool ExceptionErrorMessageEnabled = true;
#endif


		/// <summary>
		/// Returns whether a random chance is met.
		/// Explained: chance in values (e.g. 3 in 100) chance of returning true
		/// </summary>
		public static bool Chance(int values, int chance = 1)
		{
			int result = new Random().Next(values);
			if (result < chance)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Returns whether this shape overlaps with another somewhere if it moves by relativeSpeed.
		/// Returns the fraction of the speed given at which they touch and the axis closest to this touching point (from either shape).
		/// The fraction will be -1f if there's an overlap but no collision caused by the relativeSpeed.
		/// </summary>
		public static bool IntersectsWith(this IShape shape1, IShape shape2, Point relativeSpeed, out float touchingAtSpeedFraction, out Point intersectionAxis)
		{
			touchingAtSpeedFraction = float.PositiveInfinity;
			intersectionAxis = Point.Zero;

			Type shape1Type = shape1.GetType();
			Type shape2Type = shape2.GetType();

			if (shape1Type == typeof(Point) && shape2Type == typeof(Point)) //2 points
			{
				Point point1 = (Point)shape1;
				Point point2 = (Point)shape2;

				//always the axis perpendicular to the speed
				intersectionAxis = new Point(relativeSpeed.Y, -relativeSpeed.X);

				if (point1 == point2)
				{
					touchingAtSpeedFraction = 0f;
					return true;
				}

				//return false if the point cannot be reached with the given speed
				if (relativeSpeed.X >= 0f && (point1.X + relativeSpeed.X < point2.X || point1.X > point2.X))
					return false;
				if (relativeSpeed.X < 0f && (point1.X + relativeSpeed.X > point2.X || point1.X < point2.X))
					return false;
				if (relativeSpeed.Y >= 0f && (point1.Y + relativeSpeed.Y < point2.Y || point1.Y > point2.Y))
					return false;
				if (relativeSpeed.Y < 0f && (point1.Y + relativeSpeed.Y > point2.Y || point1.Y < point2.Y))
					return false;

				//check if point2 is on the path of point1 + relativeSpeed
				touchingAtSpeedFraction = (point2.X - point1.X) / relativeSpeed.X;
				if (point1.Y + touchingAtSpeedFraction * relativeSpeed.Y != point2.Y)
					return false;

				return true;
			}
			else if (shape1Type == typeof(Point) && shape2Type == typeof(SimpleRectangle) || shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(Point)) //point vs AABB
			{
				Point point;
				SimpleRectangle rect;
				float xTouchingFraction, yTouchingFraction;

				if (shape1Type == typeof(Point))
				{
					point = (Point)shape1;
					rect = (SimpleRectangle)shape2;
				}
				else
				{
					point = (Point)shape2;
					rect = (SimpleRectangle)shape1;

					relativeSpeed = -relativeSpeed;
				}

				if (relativeSpeed.X >= 0f)
				{
					if (point.X + relativeSpeed.X < rect.Position1.X || point.X > rect.Position2.X)
						return false;

					xTouchingFraction = (rect.Position1.X - point.X) / relativeSpeed.X;
				}
				else
				{
					if (point.X + relativeSpeed.X > rect.Position2.X || point.X < rect.Position1.X)
						return false;

					xTouchingFraction = (rect.Position2.X - point.X) / relativeSpeed.X;
				}

				if (relativeSpeed.Y >= 0f)
				{
					if (point.Y + relativeSpeed.Y < rect.Position1.Y || point.Y > rect.Position2.Y)
						return false;

					yTouchingFraction = (rect.Position1.Y - point.Y) / relativeSpeed.Y;
				}
				else
				{
					if (point.Y + relativeSpeed.Y > rect.Position2.Y || point.Y < rect.Position1.Y)
						return false;

					yTouchingFraction = (rect.Position2.Y - point.Y) / relativeSpeed.Y;
				}

				//decide which fraction and corresponding axis to return, if any
				if (xTouchingFraction >= 0f
					&& xTouchingFraction <= 1f)
				{
					touchingAtSpeedFraction = xTouchingFraction;
					intersectionAxis = rect.GetPerpAxes()[0];
				}

				if (yTouchingFraction >= 0f
					&& yTouchingFraction <= 1f
					&& yTouchingFraction < touchingAtSpeedFraction)
				{
					touchingAtSpeedFraction = yTouchingFraction;
					intersectionAxis = rect.GetPerpAxes()[1];
				}

				if (intersectionAxis == Point.Zero)
					touchingAtSpeedFraction = -1f;

				return true;
			}
			else if (shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(SimpleRectangle)) //2 AABB's
			{
				SimpleRectangle rect1 = (SimpleRectangle)shape1;
				SimpleRectangle rect2 = (SimpleRectangle)shape2;
				float xTouchingFraction, yTouchingFraction;

				if (relativeSpeed.X >= 0f)
				{
					if (rect1.Position2.X + relativeSpeed.X < rect2.Position1.X || rect1.Position1.X > rect2.Position2.X)
						return false;

					xTouchingFraction = (rect2.Position1.X - rect1.Position2.X) / relativeSpeed.X;
				}
				else
				{
					if (rect1.Position1.X + relativeSpeed.X > rect2.Position2.X || rect1.Position2.X < rect2.Position1.X)
						return false;

					xTouchingFraction = (rect2.Position2.X - rect1.Position1.X) / relativeSpeed.X;
				}

				if (relativeSpeed.Y >= 0f)
				{
					if (rect1.Position2.Y + relativeSpeed.Y < rect2.Position1.Y || rect1.Position1.Y > rect2.Position2.Y)
						return false;

					yTouchingFraction = (rect2.Position1.Y - rect1.Position2.Y) / relativeSpeed.Y;
				}
				else
				{
					if (rect1.Position1.Y + relativeSpeed.Y > rect2.Position2.Y || rect1.Position2.Y < rect2.Position1.Y)
						return false;

					yTouchingFraction = (rect2.Position2.Y - rect1.Position1.Y) / relativeSpeed.Y;
				}

				//decide which fraction and corresponding axis to return, if any
				if (xTouchingFraction >= 0f 
					&& xTouchingFraction <= 1f)
				{
					touchingAtSpeedFraction = xTouchingFraction;
					intersectionAxis = rect2.GetPerpAxes()[0];
				}
				
				if (yTouchingFraction >= 0f
					&& yTouchingFraction <= 1f
					&& yTouchingFraction < touchingAtSpeedFraction)
				{
					touchingAtSpeedFraction = yTouchingFraction;
					intersectionAxis = rect2.GetPerpAxes()[1];
				}

				if (intersectionAxis == Point.Zero)
					touchingAtSpeedFraction = -1f;

				return true;
			}
			else //other shapes
			{
				touchingAtSpeedFraction = float.PositiveInfinity;

				Point[] shape1Points = shape1.GetPoints();
				Point[] shape2Points = shape2.GetPoints();

				List<Point> allAxes = new List<Point>(shape1.GetPerpAxes());
				allAxes.AddRange(shape2.GetPerpAxes());

				foreach (Point axis in allAxes)
				{
					float shape1ScalarMin = float.PositiveInfinity;
					float shape1ScalarMax = float.NegativeInfinity;
					float shape2ScalarMin = float.PositiveInfinity;
					float shape2ScalarMax = float.NegativeInfinity;

					//cast shape1's points to the axis
					foreach (Point point in shape1Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape1ScalarMin)
							shape1ScalarMin = scalar;
						if (scalar > shape1ScalarMax)
							shape1ScalarMax = scalar;
					}

					//cast shape2's points to the axis
					foreach (Point point in shape2Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape2ScalarMin)
							shape2ScalarMin = scalar;
						if (scalar > shape2ScalarMax)
							shape2ScalarMax = scalar;
					}

					//cast speed to axis
					float speedMultiplier = (float)((relativeSpeed.X * axis.X + relativeSpeed.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float speedScalar = speedMultiplier * axis.X * axis.X + speedMultiplier * axis.Y * axis.Y;

					float thisTouchingAtSpeedFraction;
					if (speedScalar >= 0)
					{
						if (shape1ScalarMax + speedScalar < shape2ScalarMin || shape1ScalarMin > shape2ScalarMax)
							return false;

						thisTouchingAtSpeedFraction = (shape2ScalarMin - shape1ScalarMax) / speedScalar;
					}
					else
					{
						if (shape1ScalarMin + speedScalar > shape2ScalarMax || shape1ScalarMax < shape2ScalarMin)
							return false;

						thisTouchingAtSpeedFraction = (shape2ScalarMax - shape1ScalarMin) / speedScalar;
					}

					if (thisTouchingAtSpeedFraction >= 0f
						&& thisTouchingAtSpeedFraction <= 1f
						&& thisTouchingAtSpeedFraction < touchingAtSpeedFraction)
					{
						touchingAtSpeedFraction = thisTouchingAtSpeedFraction;
						intersectionAxis = axis;
					}
				}

				if (intersectionAxis == Point.Zero)
					touchingAtSpeedFraction = -1f;
				else //this axis is still a perpendicular axis, revert!
					intersectionAxis = new Point(intersectionAxis.Y, -intersectionAxis.X);

				return true;
			}
		}
		/// <summary>
		/// Returns whether this shape overlaps with another somewhere if it moves by relativeSpeed.
		/// </summary>
		public static bool IntersectsWith(this IShape shape1, IShape shape2, Point relativeSpeed)
		{
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			return IntersectsWith(shape1, shape2, relativeSpeed, out touchingAtSpeedFraction, out intersectionAxis);
		}
		/// <summary>
		/// Returns whether this shape overlaps with another.
		/// </summary>
		public static bool IntersectsWith(this IShape shape1, IShape shape2)
		{
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			return IntersectsWith(shape1, shape2, Point.Zero, out touchingAtSpeedFraction, out intersectionAxis);
		}
	}
}