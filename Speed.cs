using System;

namespace HatlessEngine
{
    /// <summary>
    /// Stores a speed vector.
    /// Can check by horizontal and vertical, or direction and amplitude.
    /// </summary>
    public struct Speed
    {
        public float X;
        public float Y;

        public Speed(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Direction
        {
            get 
            {
                float result;
                if (X == 0 && Y == 0)
                    result = 0;
                else
                {
                    result = (float)(Math.Atan2(X, -Y) * 180 / Math.PI);
                    if (result < 0)
                        result = result + 360;
                }
                return result;
            }
        }
        public float Amplitude
        {
            get { return (float)Math.Abs(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2))); }
        }
    }
}
