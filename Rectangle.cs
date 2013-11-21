namespace HatlessEngine
{
    /// <summary>
    /// Defines a position and size.
    /// You can also access the absolute position of the bottom right corner individually.
    /// </summary>
    public struct Rectangle
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float X2
        {
            set { Width = value - X; }
            get { return X + Width; }
        }
        public float Y2
        {
            set { Height = value - Y; }
            get { return Y + Height; }
        }

        public Position Position
        {
            get { return new Position(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public Size Size
        {
            get { return new Size(Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public Rectangle(Position pos, Size size) : this(pos.X, pos.Y, size.Width, size.Height) { }

        public static Rectangle operator +(Rectangle rect, Position pos)
        {
            return new Rectangle(rect.X + pos.X, rect.Y + pos.Y, rect.Width, rect.Height);
        }
        public static Rectangle operator -(Rectangle rect, Position pos)
        {
            return new Rectangle(rect.X - pos.X, rect.Y - pos.Y, rect.Width, rect.Height);
        }

        public static implicit operator SFML.Graphics.FloatRect(Rectangle rect)
        {
            return new SFML.Graphics.FloatRect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}