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
    }
}
