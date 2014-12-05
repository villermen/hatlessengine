using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	public class Sprite : IExternalResource
	{   
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public string Id { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr TextureHandle;

		/// <summary>
		/// Every entry is an animation.
		/// An identifier and an array of any amount of frames in the order you desire.
		/// </summary>
		public Dictionary<string, int[]> Animations = new Dictionary<string, int[]>();

		private readonly bool _autoSize = false;
		public Point FrameSize { get; private set; }
		public Point IndexSize { get; private set; }
		
		public Sprite(string id, string filename) 
			: this(id, filename, new Point(1f, 1f))
		{
			_autoSize = true;
			FileAssembly = Assembly.GetCallingAssembly();
		}
		public Sprite(string id, string filename, Point frameSize)
		{
			Id = id;
			Filename = filename;
			FileAssembly = Assembly.GetCallingAssembly();
			
			Loaded = false;

			FrameSize = frameSize;
			IndexSize = new Point(1f, 1f);

			Resources.Sprites.Add(Id, this);
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
			return new Point(Misc.Modulus(frameIndex, (int)IndexSize.X) * FrameSize.X, Misc.Modulus((int)(frameIndex / IndexSize.X), (int)IndexSize.Y) * FrameSize.Y);
		}

		public void Load()
		{
			if (Loaded)
				return;

			IntPtr surface = SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(Filename, FileAssembly), 1);
			if (surface != IntPtr.Zero)
			{
				TextureHandle = SDL.SDL_CreateTextureFromSurface(Game.RendererHandle, surface);
				SDL.SDL_FreeSurface(surface);

				if (TextureHandle != IntPtr.Zero)
				{
					uint format;
					int access, w, h;
					SDL.SDL_QueryTexture(TextureHandle, out format, out access, out w, out h);

					if (_autoSize)
					{
						FrameSize = new Point(w, h);
						IndexSize = new Point(1, 1);
					}
					else
						IndexSize = new Point((float)Math.Floor(w / FrameSize.X), (float)Math.Floor(h / FrameSize.Y));

					Loaded = true;
				}
				else
					throw new FileLoadException(SDL.SDL_GetError());
			}
			else
				throw new FileLoadException(SDL.SDL_GetError());
		}

		public void Unload()
		{
			if (!Loaded) 
				return;

			SDL.SDL_DestroyTexture(TextureHandle);
			TextureHandle = IntPtr.Zero;
			Loaded = false;
		}

		public void Destroy()
		{
			Unload();

			Resources.Sprites.Remove(Id);
			Resources.ExternalResources.Remove(this);
		}

		public static implicit operator Sprite(string str)
		{
			return Resources.Sprites[str];
		}

		protected virtual void Dispose(bool disposing)
		{
			//destroy either way, this overload is just for convention
			Destroy();
		}

		/// <summary>
		/// Pretty much an alias for Destroy(), here just to implement IDisposable as this object uses unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			//do not suppress finalization as the resource could be loaded after this point
		}

		~Sprite()
		{
			Dispose(false);
		}
	}
}