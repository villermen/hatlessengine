using System;

namespace HatlessEngine
{
	[Serializable]
	public struct Color
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public Color(byte red, byte green, byte blue, byte alpha = 255)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		public static readonly Color Transparent = new Color(0, 0, 0, 0);
		public static readonly Color Black = new Color(0, 0, 0);

		public static readonly Color White = new Color(255, 255, 255);
		public static readonly Color Red = new Color(255, 0, 0);
		public static readonly Color Green = new Color(0, 255, 0);
		public static readonly Color Blue = new Color(0, 0, 255);
		public static readonly Color Yellow = new Color(255, 255, 0);
		public static readonly Color Magenta = new Color(255, 0, 255);
		public static readonly Color Cyan = new Color(0, 255, 255);

		public static readonly Color Gray = new Color(128, 128, 128);

		public static explicit operator System.Drawing.Color(Color color)
		{
			return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		}
		public static explicit operator OpenTK.Graphics.Color4(Color color)
		{
			return new OpenTK.Graphics.Color4(color.R, color.G, color.B, color.A);
		}
	}
}

