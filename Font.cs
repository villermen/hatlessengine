using System;

namespace HatlessEngine
{
    public class Font : IExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool IsLoaded { get; private set; }

        internal SFML.Graphics.Text SFMLText;

        internal Font(string id, string filename)
        {
            Id = id;
            Filename = filename;
        }

        public void Draw(string str, Position pos, uint size, Color color)
        {
            Load();
            SFMLText.DisplayedString = str;
            SFMLText.Position = new SFML.Window.Vector2f(pos.X, pos.Y);
            SFMLText.CharacterSize = size;
            SFMLText.Color = color;
            Resources.RenderPlane.Draw(SFMLText);
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                SFMLText = new SFML.Graphics.Text("", new SFML.Graphics.Font(Filename));
                IsLoaded = true;
            }
        }

        public void Unload()
        {
            SFMLText.Dispose();
            SFMLText = null;
            IsLoaded = false;
        }
    }
}
