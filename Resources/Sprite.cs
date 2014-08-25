using SDL2;
using SDL2_image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HatlessEngine
{
	public class Sprite : IExternalResource
	{   
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public string ID { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr TextureHandle;

		/// <summary>
		/// Every entry is an animation.
		/// An identifier and an array of any amount of frames in the order you desire.
		/// </summary>
		public Dictionary<string, int[]> Animations = new Dictionary<string, int[]>();

		private bool AutoSize = false;
		public Point FrameSize { get; private set; }
		public Point IndexSize { get; private set; }
		
		public Sprite(string id, string filename) 
			: this(id, filename, new Point(1f, 1f))
		{
			AutoSize = true;
			FileAssembly = Assembly.GetCallingAssembly();
		}
		public Sprite(string id, string filename, Point frameSize)
		{
			ID = id;
			Filename = filename;
			FileAssembly = Assembly.GetCallingAssembly();
			
			Loaded = false;

			FrameSize = frameSize;
			IndexSize = new Point(1f, 1f);

			Resources.Sprites.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public void Draw(ComplexRectangle rect, int frameIndex = 0, int depth = 0)
		{
			if (!Loaded)
				throw new NotLoadedException();

			Rectangle sourceRect = new Rectangle(GetIndexLocation(frameIndex), FrameSize);
			DrawX.DrawJobs.Add(new TextureDrawJob(depth, rect.GetEnclosingRectangle(), TextureHandle, sourceRect, rect));
		}
		public void Draw(Point pos, Point scale, Point origin, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Draw(new ComplexRectangle(pos, FrameSize * scale, origin, rotation), frameIndex, depth);
		}
		public void Draw(Point pos, Point scale, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Draw(new ComplexRectangle(pos, FrameSize * scale, Point.Zero, rotation), frameIndex, depth);
		}
		public void Draw(Point pos, int frameIndex = 0, float rotation = 0f, int depth = 0)
		{
			Draw(new ComplexRectangle(pos, FrameSize), frameIndex, depth);
		}

		private Point GetIndexLocation(int frameIndex)
		{
			return new Point(Misc.Modulus(frameIndex, (int)IndexSize.X) * FrameSize.X, (float)(Misc.Modulus((int)(frameIndex / IndexSize.X), (int)IndexSize.Y) * FrameSize.Y));
		}

		public void Load()
		{
			if (!Loaded)
			{
				IntPtr surface = IMG.Load_RW(Resources.CreateRWFromFile(Filename, FileAssembly), 1);
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
							IndexSize = new Point((float)Math.Floor(w / FrameSize.X), (float)Math.Floor(h / FrameSize.Y));

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

		public void Destroy()
		{
			Unload();

			Resources.Sprites.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}
	}
}