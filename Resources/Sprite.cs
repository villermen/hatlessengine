using SDL2;
using SDL2_image;
using System;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
	public class Sprite : IExternalResource
	{   
		public string Filename { get; private set; }
		public string ID { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr TextureHandle;

		internal Dictionary<string, int[]> Animations = new Dictionary<string, int[]>();

		private bool AutoSize = false;
		public Point FrameSize { get; private set; }
		public Point IndexSize { get; private set; }
		
		public Sprite(string id, string filename) 
			: this(id, filename, new Point(1f, 1f))
		{
			AutoSize = true;
		}
		public Sprite(string id, string filename, Point frameSize)
		{
			ID = id;
			Filename = filename;
			Loaded = false;

			FrameSize = frameSize;
			IndexSize = new Point(1f, 1f);

			Resources.Sprites.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public void Draw(Point pos, Point scale, Point origin, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			if (!Loaded)
				throw new NotLoadedException();

			Rectangle sourceRect = new Rectangle(GetIndexLocation(frameIndex), FrameSize);
			ComplexRectangle destRect = new ComplexRectangle(pos, FrameSize * scale, origin, rotation);

			DrawX.DrawJobs.Add(new TextureDrawJob(depth, destRect.GetEnclosingRectangle(), TextureHandle, sourceRect, destRect));
		}
		public void Draw(Point pos, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Draw(pos, new Point(1f, 1f), new Point(0f, 0f), frameIndex, rotation, depth);
		}

		private Point GetIndexLocation(int frameIndex)
		{
			return new Point(frameIndex % IndexSize.X * FrameSize.X, (float)(Math.Floor(frameIndex / IndexSize.X) % IndexSize.Y * FrameSize.Y));
		}

		public void Load()
		{
			if (!Loaded)
			{
				IntPtr surface = IMG.Load_RW(Resources.CreateRWFromFile(Filename), 1);
				if (surface != IntPtr.Zero)
				{
					TextureHandle = SDL.CreateTextureFromSurface(Game.RendererHandle, surface);
					SDL.FreeSurface(surface);

					if (TextureHandle != IntPtr.Zero)
					{
						uint format;
						int access, w, h;
						SDL.QueryTexture(TextureHandle, out format, out access, out w, out h);

						if (AutoSize)
						{
							FrameSize = new Point(w, h);
							IndexSize = new Point(1, 1);
						}
						else
							IndexSize = new Point(w / FrameSize.X, h / FrameSize.Y);

						Loaded = true;
					}
					else
						throw new FileLoadException(SDL.GetError());
				}
				else
					throw new FileLoadException(SDL.GetError());
			}
		}

		public void Unload()
		{
			if (Loaded)
			{
				SDL.DestroyTexture(TextureHandle);
				TextureHandle = IntPtr.Zero;
				Loaded = false;
			}
		}

		public void AddAnimation(string id, int[] animation)
		{
			//add error catching
			Animations.Add(id, animation);
		}
		public void AddAnimation(string id, int startIndex, int frames)
		{
			int[] animationArray = new int[frames];

			for (int i = 0; i < frames; i++)
			{
				animationArray[i] = startIndex + i;
			}

			Animations.Add(id, animationArray);
		}

		public void Destroy()
		{
			Unload();

			Resources.Sprites.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}
	}
}