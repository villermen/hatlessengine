using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace HatlessEngine
{
    public class Sprite : ExternalResource
    {   
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool Loaded { get; private set; }

		private int OpenGLTextureId;

        internal Dictionary<string, uint[]> Animations = new Dictionary<string, uint[]>();

        private bool AutoSize = false;
		public Size FrameSize { get; private set; }
		public Size IndexSize { get; private set; }
        
		internal Sprite(string id, string filename) : this(id, filename, Size.Empty)
        {
            AutoSize = true;
        }
		internal Sprite(string id, string filename, Size frameSize)
        {
            Id = id;
            Filename = filename;
            Loaded = false;

			FrameSize = frameSize;
			IndexSize = Size.Empty;
        }

		public void Draw(PointF pos, uint frameIndex, SizeF scale)
        {
			if (!Loaded)
			{
				if (Settings.JustInTimeResourceLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			float x1 = 1f / IndexSize.Width * (frameIndex % IndexSize.Height);
			float x2 = 1f / IndexSize.Width * (frameIndex % IndexSize.Height + 1);
			float y1 = 1f / IndexSize.Height * (frameIndex / IndexSize.Height);
			float y2 = 1f / IndexSize.Height * (frameIndex / IndexSize.Height + 1);

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.BindTexture(TextureTarget.Texture2D, OpenGLTextureId);
			GL.Color3(Color.White);

			GL.Begin(PrimitiveType.Quads);

			GL.TexCoord2(x1, y1);
			GL.Vertex3(pos.X, pos.Y, DrawX.GLDepth);
			GL.TexCoord2(x2, y1);
			GL.Vertex3(pos.X + FrameSize.Width * scale.Width, pos.Y, DrawX.GLDepth);
			GL.TexCoord2(x2, y2);
			GL.Vertex3(pos.X + FrameSize.Width * scale.Width, pos.Y + FrameSize.Height * scale.Height, DrawX.GLDepth);
			GL.TexCoord2(x1, y2);
			GL.Vertex3(pos.X, pos.Y + FrameSize.Height * scale.Height, DrawX.GLDepth);

			GL.End();
        }
		public void Draw(PointF pos)
		{
			Draw(pos, 0, new SizeF(1, 1));
		}
		public void Draw(PointF pos, uint frameIndex)
		{
			Draw(pos, frameIndex, new SizeF(1, 1));
		}
		public void Draw(PointF pos, uint frameIndex, float scale)
		{
			Draw(pos, frameIndex, new SizeF(scale, scale));
		}

        public void Load()
        {
			if (!Loaded)
			{
				Bitmap bitmap = new Bitmap(Filename);
				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				OpenGLTextureId = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, OpenGLTextureId);

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
				bitmap.UnlockBits(bitmapData);
				bitmap.Dispose();

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

				if (AutoSize)
				{
					FrameSize = new Size(bitmapData.Width, bitmapData.Height);
					IndexSize = new Size(1, 1);
				}
				else
				{
					IndexSize = new Size(bitmapData.Width / FrameSize.Width, bitmapData.Height / FrameSize.Height);
				}
                    
				Loaded = true;
			}
        }

        public void Unload()
        {
			if (Loaded)
			{
				GL.DeleteTexture(OpenGLTextureId);
				Loaded = false;
			}
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
