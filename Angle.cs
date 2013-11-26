using System;

namespace HatlessEngine
{
    public struct Angle
    {
        private float _Degrees;
        public float Degrees
        {
            get 
            {
                return _Degrees;
            }
            set
            {
                _Degrees = (value % 360 + 360) % 360;
            }
        }

        public float Radians
        {
            get
            {
                return _Degrees * (float)Math.PI / 180;
            }
            set
            {
                Degrees = value / (float)Math.PI * 180;
            }
        }

        public Angle(float degrees)
        {
            _Degrees = 0;
            Degrees = degrees; 
        }
        public Angle(float radians, bool isRadian)
        {
            _Degrees = 0;
            Radians = radians;
        }
    }
}
