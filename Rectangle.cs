namespace HatlessEngine
{
    public class Rectangle
    {

        private float _Width;
        private float _Height;
        private float _X2;
        private float _Y2;
        public float X;
        public float Y;
        public float Width
        {
            set
            {
                _X2 = X + value;
                _Width = value;
            }
            get { return _Width; }
        }
        public float Height
        {
            set
            {
                _Y2 = Y + value;
                _Height = value;
            }
            get { return _Height; }
        }
        public float X2
        {
            set
            {
                _Width = value - X;
                _X2 = value;
            }
            get { return _X2; }
        }
        public float Y2
        {
            set
            {
                _Height = value - Y;
                _Y2 = value;
            }
            get { return _Y2; }
        }

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}