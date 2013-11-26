using System;

namespace HatlessEngine
{
    /// <summary>
    /// Static class used to draw strings and shapes.
    /// DrawExtra (Because objects already have a Draw method and thus cant call the Draw class. There you go, hope I cleared that up for y'all.)
    /// </summary>
    public static class DrawX
    {
        public static void Text(string str, string fontId, Position pos, Color color, uint fontSize = 12)
        {
            Resources.Fonts[fontId].Draw(str, pos, fontSize, color);
        }

        public static void Rectangle(Rectangle rect, Color color)
        {
            SFML.Graphics.RectangleShape sfmlRect = new SFML.Graphics.RectangleShape(rect.Size);
            sfmlRect.Position = rect.Position;
            sfmlRect.FillColor = color;
            Resources.RenderPlane.Draw(sfmlRect);
        }

        public static void Line(Position pos1, Position pos2, Color color)
        {
            SFML.Graphics.VertexArray sfmlLine = new SFML.Graphics.VertexArray(SFML.Graphics.PrimitiveType.LinesStrip, 2);
            sfmlLine[0] = new SFML.Graphics.Vertex(pos1, color);
            sfmlLine[1] = new SFML.Graphics.Vertex(pos2, color);
            Resources.RenderPlane.Draw(sfmlLine);
        }
    }
}
