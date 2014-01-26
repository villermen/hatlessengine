using System;
using System.Drawing;

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

		public static float DistanceBetweenPoints(PointF point1, PointF point2)
		{
			return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));  
		}

		public static float AngleBetweenPoints(PointF point1, PointF point2)
		{
			float result = (float)(Math.Atan2(point2.X - point1.X, point1.Y - point2.Y) / Math.PI * 180);
			if (result < 0)
			{
				result += 360;
			}
			return result;
		}

		/// <summary>
		/// Calculates position of a point if it is rotated over an absolute origin by [angle] degrees.
		/// </summary>
		/// <param name="point">Position of point.</param>
		/// <param name="origin">Absolute origin.</param>
		/// <param name="angle">Degrees to rotate.</param>
		public static PointF RotatePointOverOrigin(PointF point, PointF origin, float angle)
		{
			//convert angle to radians
			angle = (float)(Math.PI / 180  * angle);

			float newX = (float)(origin.X + (point.X - origin.X) * Math.Cos(angle) - (point.Y - origin.Y) * Math.Sin(angle));
			float newY = (float)(origin.Y + (point.Y - origin.Y) * Math.Cos(angle) + (point.X - origin.X) * Math.Sin(angle));

			return new PointF(newX, newY);
		}
    }
}
