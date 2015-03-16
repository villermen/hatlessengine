using System;
using System.IO;
using SDL2;

namespace HatlessEngine
{
	public class Sprite : ExternalResource
	{   
		internal IntPtr TextureHandle;

		public readonly bool AutoSize;
		public Point FrameSize { get; private set; }
		public Point IndexSize { get; private set; }

		/// <summary>
		/// Create the sprite as a full image sprite (framesize 1,1).
		/// </summary>
		public Sprite(string id, string file) 
			: this(id, file, new Point(1f, 1f))
		{
			AutoSize = true;
		}

		/// <summary>
		/// Create the sprite with a custom frame size.
		/// </summary>
		public Sprite(string id, string file, Point frameSize)
			: base(id, file)
		{
			FrameSize = frameSize;
			IndexSize = new Point(1f, 1f);
		}

		/// <summary>
		/// Draw the sprite projected on a rectangle.
		/// </summary>
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

		public override void Load()
		{
			if (Loaded)
				return;

			IntPtr surface = SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(File, FileAssembly), 1);
			if (surface != IntPtr.Zero)
			{
				TextureHandle = SDL.SDL_CreateTextureFromSurface(Game.RendererHandle, surface);
				SDL.SDL_FreeSurface(surface);

				if (TextureHandle != IntPtr.Zero)
				{
					uint format;
					int access, w, h;
					SDL.SDL_QueryTexture(TextureHandle, out format, out access, out w, out h);

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
					throw new FileLoadException(SDL.SDL_GetError());
			}
			else
				throw new FileLoadException(SDL.SDL_GetError());
		}

		public override void Unload()
		{
			if (!Loaded) 
				return;

			SDL.SDL_DestroyTexture(TextureHandle);
			TextureHandle = IntPtr.Zero;
			Loaded = false;
		}

		public static implicit operator Sprite(string id)
		{
			return Resources.Get<Sprite>(id);
		}
	}
}