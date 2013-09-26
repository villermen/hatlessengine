using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class Sprite
    {   
        public string Filename { get; private set; }
        public string Id { get; internal set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Graphics.Sprite SFMLSprite;

        private Dictionary<string, uint[]> Animations = new Dictionary<string, uint[]>();
        private string CurrentAnimation = "";
        private uint AnimationIndex = 0;

        private bool AutoSize = false;

        private float width = 0;
        private float height = 0;
        public float Width 
        {
            get
            {
                Load();
                return width;
            }
            private set
            {
                width = value;
            }
        }
        public float Height
        {
            get
            {
                Load();
                return height;
            }
            private set
            {
                height = value;
            }
        }
        
        public Sprite(string filename) : this(filename, 0, 0)
        {
            AutoSize = true;
        }
        public Sprite(string filename, float width, float height)
        {
            Width = width;
            Height = height;
            Filename = filename;
            IsLoaded = false;
        }

        public void Draw(float x, float y)
        {
            Load();
            SFMLSprite.Position = new SFML.Window.Vector2f(x, y);
            Game.RenderPlane.Draw(SFMLSprite);
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                SFMLSprite = new SFML.Graphics.Sprite(new SFML.Graphics.Texture(Filename));
                if (AutoSize)
                {
                    width = SFMLSprite.GetLocalBounds().Width;
                    height = SFMLSprite.GetLocalBounds().Height;
                }
                    
                IsLoaded = true;
            }
        }

        public void Unload()
        {
            SFMLSprite = null;
            IsLoaded = false;
        }

        public void AddAnimation(string id, uint[] animation)
        {
        }
        public void AddAnimation(string id, uint startIndex, uint frames)
        {
        }
        public void StartAnimation(string id, bool loop)
        {
        }
    }
}
