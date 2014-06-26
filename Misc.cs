using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	public static class Misc
	{
		/// <summary>
		/// Returns whether a random chance is met.
		/// Explained: chance in values (1 in 100) chance of returning true
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
		/// Returns whether two shapes overlap.
		/// </summary>
		public static bool ShapesIntersecting(IShape shape1, IShape shape2)
		{
			Type shape1Type = shape1.GetType();
			Type shape2Type = shape2.GetType();

			if (shape1Type == typeof(Point) && shape2Type == typeof(Point)) //2 points
			{
				if (shape1 == shape2)
					return true;
				else
					return false;
			}
			else if (shape1Type == typeof(Point) && shape2Type == typeof(SimpleRectangle) || shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(Point)) //point vs AABB
			{
				Point point;
				SimpleRectangle rect;

				if (shape1Type == typeof(Point))
				{
					point = (Point)shape1;
					rect = (SimpleRectangle)shape2;
				}
				else
				{
					point = (Point)shape2;
					rect = (SimpleRectangle)shape1;
				}

				if (point.X < rect.Position1.X)
					return false;
				if (point.X > rect.Position2.X)
					return false;
				if (point.Y < rect.Position1.Y)
					return false;
				if (point.Y > rect.Position2.Y)
					return false;
				return true;
			}
			else if (shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(SimpleRectangle)) //2 AABB's
			{
				SimpleRectangle rect1 = (SimpleRectangle)shape1;
				SimpleRectangle rect2 = (SimpleRectangle)shape2;

				if (rect1.Position2.X < rect2.Position1.X)
					return false;
				if (rect1.Position1.X > rect2.Position2.X)
					return false;
				if (rect1.Position2.Y < rect2.Position1.Y)
					return false;
				if (rect1.Position1.Y > rect2.Position2.Y)
					return false;
				return true;
			}
			else //other shapes
			{
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

					foreach (Point point in shape1Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape1ScalarMin)
							shape1ScalarMin = scalar;
						if (scalar > shape1ScalarMax)
							shape1ScalarMax = scalar;
					}
					foreach (Point point in shape2Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape2ScalarMin)
							shape2ScalarMin = scalar;
						if (scalar > shape2ScalarMax)
							shape2ScalarMax = scalar;
					}

					//something does not overlap -> not intersecting
					if (shape1ScalarMax < shape2ScalarMin || shape1ScalarMin > shape2ScalarMax)
						return false;
				}
				//everything´s overlapping -> intersecting
				return true;
			}
		}

		/// <summary>
		/// Returns if, when and at what axis two given shapes are going to intersect using their speed.
		/// </summary>
		public static bool ShapesIntersectingBySpeed(IShape shape1, IShape shape2, Point relativeSpeed, out float touchingAtSpeedFraction, out Point intersectionAxis)
		{
			touchingAtSpeedFraction = 1f;
			intersectionAxis = Point.Zero;

			if (relativeSpeed == Point.Zero)
				return false;

			Type shape1Type = shape1.GetType();
			Type shape2Type = shape2.GetType();

			if (shape1Type == typeof(Point) && shape2Type == typeof(Point)) //2 points
				return false; //i guess
			else if (shape1Type == typeof(Point) && shape2Type == typeof(SimpleRectangle) || shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(Point))
			{
				throw new NotImplementedException();
			}
			else if (shape1Type == typeof(SimpleRectangle) && shape2Type == typeof(SimpleRectangle)) //2 AABB's
			{
				SimpleRectangle rect1 = (SimpleRectangle)shape1;
				SimpleRectangle rect2 = (SimpleRectangle)shape2;

				Point positiveOverlap = rect1.Position2 + relativeSpeed - rect2.Position1;
				Point negativeOverlap = rect1.Position1 + relativeSpeed - rect2.Position2;

				if (positiveOverlap.X > 0f && positiveOverlap.X < relativeSpeed.X)
				{
					if (rect1.Position2.Y + relativeSpeed.Y >= rect2.Position1.Y && rect1.Position1.Y + relativeSpeed.Y <= rect2.Position2.Y)
					{
						touchingAtSpeedFraction = (relativeSpeed.X - positiveOverlap.X) / relativeSpeed.X;
						intersectionAxis = rect2.GetPerpAxes()[1];
					}
				}
				if (positiveOverlap.Y > 0f && positiveOverlap.Y < relativeSpeed.Y)
				{
					if (rect1.Position2.X + relativeSpeed.X >= rect2.Position1.X && rect1.Position1.X + relativeSpeed.X <= rect2.Position2.X)
					{
						float fraction = (relativeSpeed.Y - positiveOverlap.Y) / relativeSpeed.Y;
						if (fraction < touchingAtSpeedFraction)
						{
							touchingAtSpeedFraction = fraction;
							intersectionAxis = rect2.GetPerpAxes()[0];
						}
					}
				}
				if (negativeOverlap.X < 0f && negativeOverlap.X > relativeSpeed.X)
				{
					if (rect1.Position2.Y + relativeSpeed.Y >= rect2.Position1.Y && rect1.Position1.Y + relativeSpeed.Y <= rect2.Position2.Y)
					{
						float fraction = (relativeSpeed.X - negativeOverlap.X) / relativeSpeed.X;
						if (fraction < touchingAtSpeedFraction)
						{
							touchingAtSpeedFraction = fraction;
							intersectionAxis = rect2.GetPerpAxes()[1];
						}
					}
				}
				if (negativeOverlap.Y < 0f && negativeOverlap.Y > relativeSpeed.Y)
				{
					if (rect1.Position2.X + relativeSpeed.X >= rect2.Position1.X && rect1.Position1.X + relativeSpeed.X <= rect2.Position2.X)
					{
						float fraction = (relativeSpeed.Y - negativeOverlap.Y) / relativeSpeed.Y;
						if (fraction < touchingAtSpeedFraction)
						{
							touchingAtSpeedFraction = fraction;
							intersectionAxis = rect2.GetPerpAxes()[0];
						}
					}
				}

				if (intersectionAxis != Point.Zero)
					return true;

				return false;
			}
			else //other shapes
			{
				bool fractionFound = false;
				Point[] shape1Points = shape1.GetPoints();
				Point[] shape2Points = shape2.GetPoints();
				List<Point> allAxes = new List<Point>(shape1.GetPerpAxes());
				allAxes.AddRange(shape2.GetPerpAxes());

				float multiplier;

				foreach (Point axis in allAxes)
				{
					float shape1ScalarMin = float.PositiveInfinity;
					float shape1ScalarMax = float.NegativeInfinity;
					float shape2ScalarMin = float.PositiveInfinity;
					float shape2ScalarMax = float.NegativeInfinity;

					foreach (Point point in shape1Points)
					{
						multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape1ScalarMin)
							shape1ScalarMin = scalar;
						if (scalar > shape1ScalarMax)
							shape1ScalarMax = scalar;
					}
					foreach (Point point in shape2Points)
					{
						multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape2ScalarMin)
							shape2ScalarMin = scalar;
						if (scalar > shape2ScalarMax)
							shape2ScalarMax = scalar;
					}

					//cast speed to axis
					multiplier = (float)((relativeSpeed.X * axis.X + relativeSpeed.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float speedScalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					float thisTouchingAtSpeedFraction;
					if (speedScalar >= 0)
					{
						if (shape1ScalarMax + speedScalar < shape2ScalarMin || shape1ScalarMin > shape2ScalarMax)
							return false;

						thisTouchingAtSpeedFraction = (shape2ScalarMin - shape1ScalarMax) / Math.Abs(speedScalar);
					}
					else
					{
						if (shape1ScalarMin + speedScalar > shape2ScalarMax || shape1ScalarMax < shape2ScalarMin)
							return false;

						thisTouchingAtSpeedFraction = (shape1ScalarMin - shape2ScalarMax) / Math.Abs(speedScalar);
					}

					if (thisTouchingAtSpeedFraction <= touchingAtSpeedFraction && thisTouchingAtSpeedFraction >= -0.00001f && thisTouchingAtSpeedFraction <= 1f)
					{
						//float precision error passthrough bug
						if (thisTouchingAtSpeedFraction < 0f)
							thisTouchingAtSpeedFraction = 0f;

						touchingAtSpeedFraction = thisTouchingAtSpeedFraction;
						intersectionAxis = axis;
						fractionFound = true;
					}
				}

				return fractionFound;
			}
		}
	}
}