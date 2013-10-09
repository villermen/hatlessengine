using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class Sprite
    {   
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Graphics.Sprite SFMLSprite;

        internal Dictionary<string, uint[]> Animations = new Dictionary<string, uint[]>();

        private bool AutoWidth = false;

        private Size _Size;
        public Size Size
        {
            get
            {
                Load();
                return _Size;
            }
            private set
            {
                _Size = value;
            }
        }
        
        public Sprite(string id, string filename) : this(id, filename, 0)
        {
            AutoWidth = true;
        }
        public Sprite(string id, string filename, uint width)
        {
            Id = Id;
            Size = new Size((float)width, 0);
            Filename = filename;
            IsLoaded = false;
        }

        public void Draw(float x, float y, uint frameIndex = 0)
        {
            Load();
            SFMLSprite.Position = new SFML.Window.Vector2f(x, y);
            SFMLSprite.TextureRect = new SFML.Graphics.IntRect((int)(frameIndex * Size.Width), 0, (int)Size.Width, (int)Size.Height);
            Game.RenderPlane.Draw(SFMLSprite);
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                SFMLSprite = new SFML.Graphics.Sprite(new SFML.Graphics.Texture(Filename));
                float height = (uint)SFMLSprite.GetLocalBounds().Height;

                float width;
                if (AutoWidth)                   
                    width = (uint)SFMLSprite.GetLocalBounds().Width;
                else
                    width = Size.Width;

                Size = new Size(width, height);
                    
                IsLoaded = true;
            }
        }

        public void Unload()
        {
            SFMLSprite.Dispose();
            SFMLSprite = null;
            IsLoaded = false;
        }

        public void AddAnimation(string id, uint[] animation)
        {
            //add error catching
            Animations.Add(id, animation);
        }
        public void AddAnimation(string id, uint startIndex, uint frames)
        {
            uint[] animationArray = new uint[frames];

            for (uint i = 0; i < frames; i++)
            {
                animationArray[i] = startIndex + i;
            }

            Animations.Add(id, animationArray);
        }
    }
}
