using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace HatlessEngine
{
    public class Sprite : IExternalResource
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

		public void Draw(PointF pos, uint frameIndex, PointF scale, PointF origin, float rotation)
        {
			if (!Loaded)
			{
				if (Resources.JustInTimeLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			//calculate frame coordinates within the texture
			float texX1 = 1f / IndexSize.Width * (frameIndex % IndexSize.Height);
			float texY1 = 1f / IndexSize.Height * (frameIndex / IndexSize.Height);
			float texX2 = 1f / IndexSize.Width * (frameIndex % IndexSize.Height + 1);
			float texY2 = 1f / IndexSize.Height * (frameIndex / IndexSize.Height + 1);

			//calculate output coordinates with the transformations
			PointF absoluteOrigin = new PointF(pos.X + origin.X, pos.Y + origin.Y);

			//default positions
			PointF[] screenCoords = new PointF[4];
			screenCoords[0] = new PointF(pos.X, pos.Y);
			screenCoords[1] = new PointF(pos.X + FrameSize.Width, pos.Y);
			screenCoords[2] = new PointF(pos.X + FrameSize.Width, pos.Y + FrameSize.Height);
			screenCoords[3] = new PointF(pos.X, pos.Y + FrameSize.Height);

			//scaling
			if (scale != new PointF(1, 1))
			{
				for(byte i = 0; i < 4; i++)
				{
					screenCoords[i].X += (screenCoords[i].X - absoluteOrigin.X) * (scale.X - 1);
					screenCoords[i].Y += (screenCoords[i].Y - absoluteOrigin.Y) * (scale.Y - 1);
				}
			}

			//rotation
			if (rotation != 0)
			{
				for(byte i = 0; i < 4; i++)
				{
					screenCoords[i] = Misc.RotatePointOverOrigin(screenCoords[i], absoluteOrigin, rotation);
				}
			}

			//decide whether to actually draw it
			bool inDrawArea = false;
			for(byte i = 0; i < 4; i++)
			{
				//do a collisioncheck here (this doesnt catch the middle collision case)
				if (Game.CurrentDrawArea.Contains(screenCoords[i]))
				{
					inDrawArea = true;
					break;
				}
			}

			if (inDrawArea)
			{
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, OpenGLTextureId);
				GL.Color3(Color.White);

				GL.Begin(PrimitiveType.Quads);

				GL.TexCoord2(texX1, texY1);
				GL.Vertex3(screenCoords[0].X, screenCoords[0].Y, DrawX.GLDepth);
				GL.TexCoord2(texX2, texY1);
				GL.Vertex3(screenCoords[1].X, screenCoords[1].Y, DrawX.GLDepth);
				GL.TexCoord2(texX2, texY2);
				GL.Vertex3(screenCoords[2].X, screenCoords[2].Y, DrawX.GLDepth);
				GL.TexCoord2(texX1, texY2);
				GL.Vertex3(screenCoords[3].X, screenCoords[3].Y, DrawX.GLDepth);

				GL.End();

				GL.Disable(EnableCap.Texture2D);
			}
        }
		public void Draw(PointF pos)
		{
			Draw(pos, 0, new PointF(1, 1), new PointF(0, 0), 0);
		}
		public void Draw(PointF pos, uint frameIndex)
		{
			Draw(pos, frameIndex, new PointF(1, 1), new PointF(0, 0), 0);
		}
		public void Draw(PointF pos, uint frameIndex, float scale)
		{
			Draw(pos, frameIndex, new PointF(scale, scale), new PointF(0, 0), 0);
		}
		public void Draw(PointF pos, uint frameIndex, PointF scale)
		{
			Draw(pos, frameIndex, scale, new PointF(0, 0), 0);
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
