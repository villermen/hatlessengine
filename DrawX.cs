using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace HatlessEngine
{
    /// <summary>
    /// Static class used to draw strings and shapes.
    /// DrawExtra (Because objects already have a Draw method and thus cant call the Draw class. There you go, hope I cleared that up for y'all.)
    /// </summary>
    public static class DrawX
    {
		/// <summary>
		/// Will be used by functions that have the color argument left out.
		/// </summary>
		public static Color DefaultColor = Color.Black;

		private static int _Depth;
		/// <summary>
		/// Set the depth to draw at, will be used in all drawing related functions.
		/// Higher = further away from the viewer.
		/// Lower = closer to the viewer.
		/// </summary>
		public static int Depth
		{
			get { return _Depth; }
			set
			{
				GLDepth = (float)value / int.MaxValue;
				_Depth = value;
			}
		}
		internal static float GLDepth;

		public static void Point(PointF position, Color color, float size = 1)
		{
			GL.Disable(EnableCap.Blend);

			GL.PointSize(size);
			GL.Color3(color);

			GL.Begin(PrimitiveType.Points);

			GL.Vertex3(position.X, position.Y, GLDepth);

			GL.End();

			GL.Enable(EnableCap.Blend);
		}
		public static void Point(PointF position, float size = 1)
		{
			Point(position, DefaultColor, size);
		}

		public static void Line(PointF pos1, PointF pos2, Color color, float width = 1)
        {
			GL.Disable(EnableCap.Blend); //maybe improve on this

			GL.LineWidth(width);
			GL.Color3(color);

			GL.Begin(PrimitiveType.Lines);

			GL.Vertex3(pos1.X, pos1.Y, GLDepth);
			GL.Vertex3(pos2.X, pos2.Y, GLDepth);

			GL.End();

			GL.Enable(EnableCap.Blend);
        }
		public static void Line(PointF pos1, PointF pos2, float width = 1)
		{
			Line(pos1, pos2, DefaultColor, width);
		}

		public static void Rectangle(RectangleF rect, Color color)
		{
			GL.Disable(EnableCap.Blend);

			GL.Color3(color);

			GL.Begin(PrimitiveType.Quads);

			GL.Vertex3(rect.Left, rect.Top, GLDepth);
			GL.Vertex3(rect.Right, rect.Top, GLDepth);
			GL.Vertex3(rect.Right, rect.Bottom, GLDepth);
			GL.Vertex3(rect.Left, rect.Bottom, GLDepth);

			GL.End();

			GL.Enable(EnableCap.Blend);
		}
		public static void Rectangle(RectangleF rect)
		{
			Rectangle(rect, DefaultColor);
		}
		public static void Text(string str, string fontId, PointF position, Color color)
		{
			Resources.Fonts[fontId].Draw(str, position, color);
		}
		public static void Text(string str, string fontId, PointF position)
		{
			Text(str, fontId, position, DefaultColor);
		}
    }
}
