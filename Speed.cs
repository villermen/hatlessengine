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

        private float _Direction;

        public Speed(float x, float y)
        {
            X = x;
            Y = y;
            _Direction = 0;
        }

        public float Direction
        {
            get 
            {
                if (X == 0 && Y == 0)
                    return _Direction;

                float result = (float)(Math.Atan2(X, -Y) * 180 / Math.PI);
                if (result < 0)
                    result = result + 360;
                return result;
            }
            set
            {
                while (value >= 360)
                    value = value - 360;
                while (value < 0)
                    value = value + 360;

                _Direction = value;

                float amplitude = Amplitude;
                X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * amplitude;
                Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * amplitude;
            }
        }
        public float Amplitude
        {
            get { return (float)Math.Abs(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2))); }
            set
            {
                float direction = Direction;
                X = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * value;
                Y = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * value;
            }
        }
    }
}
