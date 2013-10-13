using System;

namespace HatlessEngine
{
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }
        public Color(byte red, byte green, byte blue) : this(red, green, blue, 255) { }

        public static implicit operator SFML.Graphics.Color(Color color)
        {
            return new SFML.Graphics.Color(color.R, color.G, color.B, color.A);
        }

        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color Yellow = new Color(255, 255, 0);
        public static readonly Color Magenta = new Color(255, 0, 255);
        public static readonly Color Cyan = new Color(0, 255, 255);
        public static readonly Color Transparent = new Color(0, 0, 0, 0);
    }
}
