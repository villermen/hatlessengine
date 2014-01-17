﻿using System;
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

		internal static float GLDepth;
		private static sbyte _Depth;
		/// <summary>
		/// Set the depth to draw at, will be used in all drawing related functions.
		/// Higher = further away from the viewer.
		/// Lower = closer to the viewer.
		/// </summary>
		public static sbyte Depth
		{
			get { return _Depth; }
			set
			{
				_Depth = value;
				GLDepth = (value + 128) / 255f;
			}
		}

		public static void Point(PointF position, Color color, float size = 1)
		{
			RectangleF pointRectangle = new RectangleF((float)Math.Floor(position.X - size * 0.5f), (float)Math.Floor(position.Y - size * 0.5f), (float)Math.Floor(size), (float)Math.Floor(size));

			if (pointRectangle.IntersectsWith(Game.CurrentDrawArea))
			{
				GL.Disable(EnableCap.Blend);

				GL.PointSize(size);
				GL.Color3(color);

				GL.Begin(PrimitiveType.Points);

				GL.Vertex3(position.X, position.Y, GLDepth);

				GL.End();

				GL.Enable(EnableCap.Blend);
			}
		}
		public static void Point(PointF position, float size = 1)
		{
			Point(position, DefaultColor, size);
		}

		public static void Line(PointF pos1, PointF pos2, Color color, float width = 1)
        {
			RectangleF lineRectangle = new RectangleF(pos1.X, pos1.Y, pos2.X - pos1.X, pos2.Y - pos1.Y);

			if (lineRectangle.IntersectsWith(Game.CurrentDrawArea))
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
        }
		public static void Line(PointF pos1, PointF pos2, float width = 1)
		{
			Line(pos1, pos2, DefaultColor, width);
		}

		public static void Rectangle(RectangleF rect, Color color)
		{
			if (rect.IntersectsWith(Game.CurrentDrawArea))
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
