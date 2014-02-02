using System;
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

		/*public static void Point(PointF position, Color color, float size = 1)
		{
			RectangleF pointRectangle = new RectangleF((float)Math.Floor(position.X - size * 0.5f), (float)Math.Floor(position.Y - size * 0.5f), (float)Math.Floor(size), (float)Math.Floor(size));

			if (pointRectangle.IntersectsWith(Game.CurrentDrawArea))
			{
				GL.PointSize(size);
				GL.Color4(color);

				GL.Begin(PrimitiveType.Points);

				GL.Vertex3(position.X, position.Y, GLDepth);

				GL.End();
			}
		}
		public static void Point(PointF position, float size = 1)
		{
			Point(position, DefaultColor, size);
		}*/
		public static void Line(Line line, Color color)
        {
			Rectangle((Rectangle)line, color);
        }
		public static void Line(Line line)
		{
			Line(line, DefaultColor);
		}

		public static void Rectangle(Rectangle rect, Color color)
		{
			if (rect.IntersectsWith(Game.CurrentDrawArea))
			{
				Point[] points = rect.Points;

				GL.Color4((OpenTK.Graphics.Color4)color);

				GL.Begin(PrimitiveType.Quads);

				GL.Vertex3(points[0].X, points[0].Y, GLDepth);
				GL.Vertex3(points[1].X, points[1].Y, GLDepth);
				GL.Vertex3(points[2].X, points[2].Y, GLDepth);
				GL.Vertex3(points[3].X, points[3].Y, GLDepth);

				GL.End();
			}
		}
		public static void Rectangle(Rectangle rect) //at position, ignoring rectangle´s actual position
		{
			Rectangle(rect, DefaultColor);
		}

		public static void Text(string str, string fontId, Point pos, Color color)
		{
			Resources.Fonts[fontId].Draw(str, pos, color);
		}
		public static void Text(string str, string fontId, Point pos)
		{
			Text(str, fontId, pos, DefaultColor);
		}

		public static void Circle(Circle circle, Color color, int segments = 90)
		{
			//calculate values used in incrementing
			float theta = (float)(2f * Math.PI / segments); 
			float cosT = (float)Math.Cos(theta);
			float sinT = (float)Math.Sin(theta);

			//start at angle = 0
			float xAdd = 0;
			float yAdd = -circle.Radius; 

			GL.Color4((OpenTK.Graphics.Color4)color);
			GL.Begin(PrimitiveType.TriangleFan);

			GL.Vertex3(circle.X, circle.Y, GLDepth);
			for(int i = 0; i < segments; i++) 
			{ 
				GL.Vertex3(circle.X + xAdd, circle.Y + yAdd, GLDepth);

				//calculate x and y for next step
				float oldXAdd = xAdd;
				xAdd = xAdd * cosT - yAdd * sinT;
				yAdd = oldXAdd * sinT + yAdd * cosT;
			}
			GL.Vertex3(circle.X, circle.Y - circle.Radius, GLDepth);

			GL.End(); 
		}
		public static void Circle(Circle circle, int segments = 360)
		{
			Circle(circle, DefaultColor, segments);
		}
    }
}
