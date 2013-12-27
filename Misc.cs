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
    }
}
