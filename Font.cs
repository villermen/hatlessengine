using System;
using SDL2;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
	public sealed class Font : IExternalResource
	{
		public string Filename { get; private set; }
		public string ID { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr Handle;

		public int PointSize { get; private set; }

		internal Dictionary<Tuple<string, Color>, IntPtr> Textures = new Dictionary<Tuple<string, Color>, IntPtr>();
		internal Dictionary<Tuple<string, Color>, int> TexturesDrawsUnused = new Dictionary<Tuple<string, Color>, int>();

		internal Font(string id, string filename, int pointSize)
		{
			ID = id;
			Filename = filename;
			Loaded = false;

			if (pointSize < 1)
				throw new ArgumentOutOfRangeException("pointSize", "pointSize must be bigger than zero.");

			PointSize = pointSize;
		}

		public void Draw(string str, Point pos, Color color, Alignment alignment = Alignment.Top | Alignment.Left, int depth = 0)
		{
			if (!Loaded)
			{
				if (Resources.JustInTimeLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			IntPtr textTexture;

			Tuple<string, Color> key = new Tuple<string, Color>(str, color);
			if (Textures.ContainsKey(key)) //use an already rendered texture
			{
				textTexture = Textures[key];
				TexturesDrawsUnused[key] = 0;
			}
			else //generate a new texture
			{
				IntPtr textSurface = SDL_ttf.TTF_RenderText_Blended(Handle, str, color);
				textTexture = SDL.SDL_CreateTextureFromSurface(Game.RendererHandle, textSurface);
				SDL.SDL_FreeSurface(textSurface);

				Textures.Add(key, textTexture);
				TexturesDrawsUnused.Add(key, 0);
			}

			uint format;
			int access, w, h;
			SDL.SDL_QueryTexture(textTexture, out format, out access, out w, out h);

			Rectangle destRect = new Rectangle(pos, new Point(w, h));

			//alignment
			if (alignment.HasFlag(Alignment.Center))
				destRect.Origin = new Point(destRect.Size.X / 2f, destRect.Origin.Y);
			if (alignment.HasFlag(Alignment.Right))
				destRect.Origin = new Point(destRect.Size.X, destRect.Origin.Y);

			if (alignment.HasFlag(Alignment.Middle))
				destRect.Origin = new Point(destRect.Origin.X, destRect.Size.Y / 2f);
			if (alignment.HasFlag(Alignment.Bottom))
				destRect.Origin = new Point(destRect.Origin.X, destRect.Size.Y);

			DrawX.DrawJobs.Add(new TextureDrawJob(depth, new SimpleRectangle(pos, new Point(w, h)), textTexture, new SimpleRectangle(Point.Zero, new Point(w, h)), destRect));
		}

		public void Load()
		{
			if (!Loaded)
			{
				if (File.Exists(Filename))
				{
					Handle = SDL_ttf.TTF_OpenFont(Filename, PointSize);

					if (Handle != IntPtr.Zero)
						Loaded = true;
					else
						throw new FileLoadException(SDL.SDL_GetError());
				}
				else
					throw new FileNotFoundException(Filename);
			}
		}

		public void Unload()
		{
			if (Loaded)
			{
				SDL_ttf.TTF_CloseFont(Handle);
				Loaded = false;
			}
		}

		~Font()
		{
			Unload();
		}
	}

	[Flags]
	public enum Alignment
	{
		None = 0,

		Left = 1,
		Center = 2,
		Right = 4,

		Top = 8,
		Middle = 16,
		Bottom = 32
	}
}