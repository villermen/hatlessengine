using System;

namespace HatlessEngine
{
    public class Font
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool IsLoaded { get; private set; }

        internal SFML.Graphics.Font SFMLFont;

        public Font(string id, string filename)
        {
            Id = id;
            Filename = filename;
        }

        public void Draw(float x, float y, string str, uint size)
        {
            Load();
            SFML.Graphics.Text text = new SFML.Graphics.Text(str, SFMLFont, size);
            text.Position = new SFML.Window.Vector2f(x, y);
            text.Color = SFML.Graphics.Color.Black;
            Game.RenderPlane.Draw(text);
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                SFMLFont = new SFML.Graphics.Font(Filename);
                IsLoaded = true;
            }
        }

        public void Unload()
        {
            SFMLFont.Dispose();
            SFMLFont = null;
            IsLoaded = false;
        }
    }
}
