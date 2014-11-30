using System;
using SDL2;

namespace HatlessEngine
{
	[Serializable]
	public struct Color
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		/// <summary>
		/// Define a color by it's RGBA components.
		/// </summary>
		public Color(byte red, byte green, byte blue, byte alpha = 255)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		/// <summary>
		/// Define a color using a value depicting one of the 16 default web-colors.
		/// </summary>
		/// <param name="number">0-15 (black, navy, green, teal, maroon, purple, olive, silver, gray, blue, lime, aqua, red, fuchsia, yellow, white)</param>
		public Color(byte number)
		{
			switch (number)
			{
				case 0:
					this = Black;
					break;
				case 1:
					this = Navy;
					break;
				case 2:
					this = Green;
					break;
				case 3:
					this = Teal;
					break;
				case 4:
					this = Maroon;
					break;
				case 5:
					this = Purple;
					break;
				case 6:
					this = Olive;
					break;
				case 7:
					this = Silver;
					break;
				case 8:
					this = Gray;
					break;
				case 9:
					this = Blue;
					break;
				case 10:
					this = Lime;
					break;
				case 11:
					this = Aqua;
					break;
				case 12:
					this = Red;
					break;
				case 13:
					this = Fuchsia;
					break;
				case 14:
					this = Yellow;
					break;
				case 15:
					this = White;
					break;
				default:
					this = Black;
					break;
			}
		}

		public Color GetComplementary()
		{
			return new Color((byte)(255 - R), (byte)(255 - G), (byte)(255 - B), A);
		}

		public static readonly Color Transparent = new Color(0, 0, 0, 0);

		public static readonly Color Black = new Color(0, 0, 0);
		public static readonly Color Navy = new Color(0, 0, 128);
		public static readonly Color Green = new Color(0, 128, 0);
		public static readonly Color Teal = new Color(0, 128, 128);
		public static readonly Color Maroon = new Color(128, 0, 0);
		public static readonly Color Purple = new Color(128, 0, 128);
		public static readonly Color Olive = new Color(128, 128, 0);
		public static readonly Color Silver = new Color(192, 192, 192);
		public static readonly Color Gray = new Color(128, 128, 128);
		public static readonly Color Blue = new Color(0, 0, 255);
		public static readonly Color Lime = new Color(0, 255, 0);
		public static readonly Color Aqua = new Color(0, 255, 255);
		public static readonly Color Red = new Color(255, 0, 0);
		public static readonly Color Fuchsia = new Color(255, 0, 255);
		public static readonly Color Yellow = new Color(255, 255, 0);
		public static readonly Color White = new Color(255, 255, 255);
		
		public static implicit operator SDL.SDL_Color(Color color)
		{
			return new SDL.SDL_Color { r = color.R, g = color.G, b = color.B, a = color.A };
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2}, {3})", R, G, B, A);
		}
	}
}