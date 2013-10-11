using System;

namespace HatlessEngine
{
    /// <summary>
    /// Defines a size of a width and height.
    /// </summary>
    public struct Size
    {
        public float Width;
        public float Height;

        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public static Size operator +(Size size1, Size size2)
        {
            return new Size(size1.Width + size2.Width, size1.Height + size2.Height);
        }
        public static Size operator -(Size size1, Size size2)
        {
            return new Size(size1.Width - size2.Width, size1.Height - size2.Height);
        }
        public static Size operator *(Size size1, Size size2)
        {
            return new Size(size1.Width * size2.Width, size1.Height * size2.Height);
        }
        public static Size operator /(Size size1, Size size2)
        {
            return new Size(size1.Width / size2.Width, size1.Height / size2.Height);
        }

        public static Size operator +(Size size, float value)
        {
            return new Size(size.Width + value, size.Height + value);
        }
        public static Size operator -(Size size, float value)
        {
            return new Size(size.Width - value, size.Height - value);
        }
        public static Size operator *(Size size, float value)
        {
            return new Size(size.Width * value, size.Height * value);
        }
        public static Size operator /(Size size, float value)
        {
            return new Size(size.Width / value, size.Height / value);
        }
    }
}
