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

        internal Dictionary<string, uint[]> Animations = new Dictionary<string, uint[]>();

        private bool AutoWidth = false;

        private uint width = 0;
        private uint height = 0;
        public uint Width 
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
        public uint Height
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
        
        public Sprite(string filename) : this(filename, 0)
        {
            AutoWidth = true;
        }
        public Sprite(string filename, uint width)
        {
            Width = width;
            Filename = filename;
            IsLoaded = false;
        }

        public void Draw(float x, float y, uint animationIndex = 0)
        {
            Load();
            SFMLSprite.Position = new SFML.Window.Vector2f(x, y);
            SFMLSprite.TextureRect = new SFML.Graphics.IntRect((int)(animationIndex * width), 0, (int)width, (int)height);
            Game.RenderPlane.Draw(SFMLSprite);
        }
        /// <summary>
        /// Draw using the settings defined in the Animation object.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="animation"></param>
        public void Draw(float x, float y, AnimatedSprite animationSpecs) 
        { 
            Draw(x, y, animationSpecs.CurrentIndex); 
        }

        public void Load()
        {
            if (!IsLoaded)
            {
                SFMLSprite = new SFML.Graphics.Sprite(new SFML.Graphics.Texture(Filename));
                height = (uint)SFMLSprite.GetLocalBounds().Height;

                if (AutoWidth)                   
                    width = (uint)SFMLSprite.GetLocalBounds().Width;
                    
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
