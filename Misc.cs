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
			List<Point> allAxes = new List<Point>(shape1.PerpAxes);
			allAxes.AddRange(shape2.PerpAxes);

			foreach(Point axis in allAxes)
			{
				float shape1ScalarMin = float.PositiveInfinity;
				float shape1ScalarMax = float.NegativeInfinity;
				float shape2ScalarMin = float.PositiveInfinity;
				float shape2ScalarMax = float.NegativeInfinity;

				foreach(Point point in shape1Points)
				{
					float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < shape1ScalarMin)
						shape1ScalarMin = scalar;
					if (scalar > shape1ScalarMax)
						shape1ScalarMax = scalar;				
				}
				foreach(Point point in shape2Points)
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

		/// <summary>
		/// Returns if, when and at what axis two given shapes are going to intersect using their speed.
		/// </summary>
		public static bool ShapesIntersectingWithSpeed(IShape shape1, IShape shape2, Point relativeSpeed, out float touchingAtSpeedFraction, out Point intersectionAxis)
		{
			touchingAtSpeedFraction = 0f;
			intersectionAxis = Point.Zero;
			Point[] shape1Points = shape1.Points;
			Point[] shape2Points = shape2.Points;
			List<Point> allAxes = new List<Point>(shape1.PerpAxes);
			allAxes.AddRange(shape2.PerpAxes);

			float multiplier;
			float minScalarOverlapValue = float.PositiveInfinity;

			foreach(Point axis in allAxes)
			{
				float shape1ScalarMin = float.PositiveInfinity;
				float shape1ScalarMax = float.NegativeInfinity;
				float shape2ScalarMin = float.PositiveInfinity;
				float shape2ScalarMax = float.NegativeInfinity;

				foreach(Point point in shape1Points)
				{
					multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

					if (scalar < shape1ScalarMin)
						shape1ScalarMin = scalar;
					if (scalar > shape1ScalarMax)
						shape1ScalarMax = scalar;				
				}
				foreach(Point point in shape2Points)
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

				float scalarOverlapValue;
				if (speedScalar >= 0)
				{
					shape1ScalarMax += speedScalar;
					scalarOverlapValue = shape1ScalarMax - shape2ScalarMin;
				}
				else
				{
					shape1ScalarMin += speedScalar;
					scalarOverlapValue = shape2ScalarMax - shape1ScalarMin; 
				}

				//does not overlap with added speed
				if (shape1ScalarMax <= shape2ScalarMin || shape1ScalarMin >= shape2ScalarMax)
					return false;

				if (scalarOverlapValue < minScalarOverlapValue && speedScalar != 0)
				{
					touchingAtSpeedFraction = 1 - scalarOverlapValue / Math.Abs(speedScalar);
					intersectionAxis = axis;
					minScalarOverlapValue = scalarOverlapValue;
				}
			}
				
			//returns true even if there was an intersection to begin with, use touchingAtSpeedFraction to detect
			return true;
		}
    }
}
