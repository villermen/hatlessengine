using System;

namespace HatlessEngine
{
    /// <summary>
    /// Static class used to draw strings and shapes.
    /// DrawExtra (Because objects already have a Draw method and thus cant call the Draw class. There you go, hope I cleared that up for y'all.)
    /// </summary>
    public static class DrawX
    {
        public static void Text(string str, Font font, Position pos, uint fontSize = 12) //color
        {
            font.Draw(str, pos, fontSize);
        }

        //shapes
    }
}
