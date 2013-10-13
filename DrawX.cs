using System;

namespace HatlessEngine
{
    /// <summary>
    /// Static class used to draw strings and shapes.
    /// DrawExtra (Because objects already have a Draw method and thus cant call the Draw class. There you go, hope I cleared that up for y'all.)
    /// </summary>
    public static class DrawX
    {
        public static void Text(string str, Font font, Position pos, Color color, uint fontSize = 12)
        {
            font.Draw(str, pos, fontSize, color);
        }

        public static void Rectangle(Rectangle rect, Color color)
        {
            SFML.Graphics.RectangleShape sfmlRect = new SFML.Graphics.RectangleShape(new SFML.Window.Vector2f(rect.Width, rect.Height));
            sfmlRect.Position = new SFML.Window.Vector2f(rect.X, rect.Y);
            sfmlRect.FillColor = color;
            Game.RenderPlane.Draw(sfmlRect);
        }

        //circles, polygons
    }
}
