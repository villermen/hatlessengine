using System;
using System.Drawing;
using QuickFont;
using OpenTK.Graphics.OpenGL;

namespace HatlessEngine
{
    public class Font : IExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool Loaded { get; private set; }

		internal QFont QuickFont;

		internal Font(string id, string filename)
		{
			Id = id;
			Filename = filename;
			Loaded = false;
		}

		public void Draw(string str, Point position, Color color)
		{
			QuickFont.Options.Colour = (OpenTK.Graphics.Color4)color;
			QuickFont.Options.Depth = DrawX.GLDepth;
			ProcessedText pText = QuickFont.ProcessText(str, float.MaxValue, QFontAlignment.Left);
			SizeF sizeF = QuickFont.Measure(pText);
			Rectangle textRect = new Rectangle(position, new Point(sizeF.Width, sizeF.Height));

			if (textRect.IntersectsWith(Game.CurrentDrawArea))
			{
				QuickFont.Print(pText, new OpenTK.Vector2(position.X, position.Y));
			}
		}
		public void Draw(string str, Point position)
		{
			Draw(str, position, DrawX.DefaultColor);
		}

        public void Load()
        {
			QuickFont = new QFont(Filename, 12);
			QuickFont.Options.UseDefaultBlendFunction = false;
			QuickFont.Options.LockToPixel = true;
        }

        public void Unload()
        {
           
        }
    }
}
