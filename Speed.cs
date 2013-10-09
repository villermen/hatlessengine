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
        
        public float Direction
        {
            get { return (float)(Math.Atan2(X, Y) / Math.PI * 180); }
            set
            {
                while (value >= 360)
                    value = value - 360;
                while (value < 0)
                    value = value + 360;

                X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * Amplitude;
                Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * Amplitude;
            }
        }
        public float Amplitude
        {
            get { return (float)Math.Sqrt(Math.Pow(X, 2) / Math.Pow(Y, 2)); }
            set
            {
                X = (float)Math.Cos((Direction / 180 - 0.5) * Math.PI) * value;
                Y = (float)Math.Sin((Direction / 180 - 0.5) * Math.PI) * value;
            }
        }
    }
}
