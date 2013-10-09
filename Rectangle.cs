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

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Replaces this rectangle's position with the given position.
        /// </summary>
        static public Rectangle operator +(Rectangle rect, Position pos)
        {
            return new Rectangle(pos.X, pos.Y, rect.Width, rect.Height);
        }
        /// <summary>
        /// Replaces this rectangle's size with the given size.
        /// </summary>
        static public Rectangle operator +(Rectangle rect, Size size)
        {
            return new Rectangle(rect.X, rect.Y, size.Width, size.Height);
        }
    }
}