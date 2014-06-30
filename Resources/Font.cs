using System;
using SDL2;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
	public class Font : IExternalResource
	{
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr Handle;

		private int LineHeight;

		public int PointSize { get; private set; }

		internal Dictionary<Tuple<string, Color>, IntPtr> Textures = new Dictionary<Tuple<string, Color>, IntPtr>();
		internal Dictionary<Tuple<string, Color>, int> TexturesDrawsUnused = new Dictionary<Tuple<string, Color>, int>();

		public Font(string id, string filename, int pointSize)
		{
			if (pointSize < 1)
				throw new ArgumentOutOfRangeException("pointSize", "pointSize must be bigger than zero.");

			ID = id;
			Filename = filename;
			Loaded = false;

			PointSize = pointSize;

			Resources.Fonts.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public void Draw(string str, Point pos, Color color, Alignment alignment = Alignment.Top | Alignment.Left, int depth = 0)
		{
			if (!Loaded)
				throw new NotLoadedException();

			string[] rows = str.Split('\n');
			IntPtr[] rowTextures = new IntPtr[rows.Length];

			//add independent drawjob for each line
			for (int i = 0; i < rows.Length; i++)
			{
				//leave blank if there's no use for a texture there (whitespace)
				if (String.IsNullOrWhiteSpace(rows[i]))
				{
					rowTextures[i] = IntPtr.Zero;
					continue;
				}

				Tuple<string, Color> key = new Tuple<string, Color>(rows[i], color);
				if (Textures.ContainsKey(key)) //use an already rendered texture
				{
					rowTextures[i] = Textures[key];
					TexturesDrawsUnused[key] = 0;
				}
				else //generate a new texture
				{
					IntPtr textSurface = SDL_ttf.TTF_RenderText_Blended(Handle, rows[i], color);
					rowTextures[i] = SDL.SDL_CreateTextureFromSurface(Game.RendererHandle, textSurface);
					SDL.SDL_FreeSurface(textSurface);

					Textures.Add(key, rowTextures[i]);
					TexturesDrawsUnused.Add(key, 0);
				}

				uint format;
				int access, w, h;
				SDL.SDL_QueryTexture(rowTextures[i], out format, out access, out w, out h);

				float horizontalOffset = 0f;
				float verticalOffset = 0f;

				//horizontal alignment
				if (alignment.HasFlag(Alignment.Center))
					horizontalOffset = -w / 2f;
				if (alignment.HasFlag(Alignment.Right))
					horizontalOffset = -w;

				//vertical alignment
				if (alignment.HasFlag(Alignment.Top))
					verticalOffset = i * LineHeight;
				if (alignment.HasFlag(Alignment.Middle))
					verticalOffset = -rows.Length * LineHeight / 2f + i * LineHeight;
				if (alignment.HasFlag(Alignment.Bottom))
					verticalOffset = -rows.Length * LineHeight + i * LineHeight;

				Point texturePos = pos + new Point(horizontalOffset, verticalOffset);
				Point textureSize = new Point(w, h);

				DrawX.DrawJobs.Add(new TextureDrawJob(depth, new SimpleRectangle(texturePos, textureSize), rowTextures[i], new SimpleRectangle(Point.Zero, new Point(w, h)), new Rectangle(texturePos, textureSize)));
			}
		}

		public void Load()
		{
			if (!Loaded)
			{
				using (BinaryReader stream = Resources.GetStream(Filename))
				{
					int length = (int)stream.BaseStream.Length;
					Handle = SDL_ttf.TTF_OpenFontRW(SDL.SDL_RWFromMem(stream.ReadBytes(length), length), 1, PointSize);

					if (Handle != IntPtr.Zero)
					{
						LineHeight = SDL_ttf.TTF_FontLineSkip(Handle);
						Loaded = true;
					}
					else
						throw new FileLoadException(SDL.SDL_GetError());
				}
			}
		}

		public void Unload()
		{
			if (Loaded)
			{
				SDL_ttf.TTF_CloseFont(Handle);
				Handle = IntPtr.Zero;
				Loaded = false;
			}
		}

		public void Destroy()
		{
			Unload();

			Resources.Fonts.Remove(ID);
			Resources.ExternalResources.Remove(this);
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