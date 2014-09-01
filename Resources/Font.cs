using SDL2;
using SDL2_ttf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HatlessEngine
{
	public class Font : IExternalResource
	{
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public bool Loaded { get; private set; }

		internal IntPtr Handle;

		public int LineHeight;

		public int PointSize { get; private set; }

		internal Dictionary<Tuple<string, Color>, IntPtr> Textures = new Dictionary<Tuple<string, Color>, IntPtr>();
		internal Dictionary<Tuple<string, Color>, int> TexturesDrawsUnused = new Dictionary<Tuple<string, Color>, int>();

		public Font(string id, string filename, int pointSize)
		{
			if (pointSize < 1)
				throw new ArgumentOutOfRangeException("pointSize", "pointSize must be bigger than zero.");

			ID = id;
			Filename = filename;
			FileAssembly = Assembly.GetCallingAssembly();
			Loaded = false;

			PointSize = pointSize;

			Resources.Fonts.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public void Draw(string str, Point pos, Color color, Alignment alignment = Alignment.TopLeft, int depth = 0)
		{
			if (!Loaded)
				throw new NotLoadedException();

			//replace tab with 4 spaces because sdl_ttf doesn't
			str = str.Replace("\t", "    ");

			//create rows by splitting on newline character
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
					IntPtr textSurface = TTF.RenderUTF8_Blended(Handle, rows[i], color);
					rowTextures[i] = SDL.CreateTextureFromSurface(Game.RendererHandle, textSurface);
					SDL.FreeSurface(textSurface);

					Textures.Add(key, rowTextures[i]);
					TexturesDrawsUnused.Add(key, 0);
				}

				uint format;
				int access, w, h;
				SDL.QueryTexture(rowTextures[i], out format, out access, out w, out h);

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

				DrawX.DrawJobs.Add(new TextureDrawJob(depth, new Rectangle(texturePos, textureSize), rowTextures[i], new Rectangle(Point.Zero, new Point(w, h)), new ComplexRectangle(texturePos, textureSize)));
			}
		}

		/// <summary>
		/// Wraps the string by adding newlines so that (if drawn) it never exceeds the maxWidth.
		/// Optionally stops after maxLines have been processed, trimming the newline character off that line.
		/// Using this method before rendering the text will ensure the text is only checked once per step instead of once per draw (which can make a big difference).
		/// <param name="charsTrimmed">Amount of chars trimmed from the end of the INPUT string.</param>
		/// </summary>
		public string WrapString(string str, float maxWidth, int maxLines, out int charsTrimmed)
		{
			if (maxWidth <= 0f)
				throw new IndexOutOfRangeException("maxWidth must be positive and nonzero.");

			if (maxLines < 1)
				throw new IndexOutOfRangeException("maxLines has to be over 0. Leave the argument out or use int.MaxValue to make the method (semi-)ignore the argument.");

			string result = "", lineRemainder = "";
			int lines = 0, lineStart = 0, lineLength = 0, w, h, length, newlinePos;

			charsTrimmed = 0;

			//replace tab with 4 spaces because sdl_ttf doesn't
			str = str.Replace("\t", "    ");

			while (lineStart + lineLength < str.Length)
			{
				lineStart = lineStart + lineLength;
				
				//calculate new line length (length til and including next newline)
				newlinePos = str.IndexOf('\n', lineStart);
				if (newlinePos != -1)
					lineLength = newlinePos + 1 - lineStart;
				else
					lineLength = str.Length - lineStart;

				lineRemainder = str.Substring(lineStart, lineLength);

				while(lineRemainder != "" && lines < maxLines)
				{
					//check if the string fits initially, because then there would be no situation right?
					TTF.SizeUTF8(Handle, lineRemainder, out w, out h);
					if (w < maxWidth)
					{
						//will be trimmed later and it's easier to detect here (because the newline could be added by wrapping too)
						if (++lines == maxLines && lineRemainder[lineRemainder.Length - 1] == '\n')
							charsTrimmed++;

						result += lineRemainder;
						lineRemainder = "";
					}
					else
					{
						//get closest fitting multiple of ten and add as many of those to the substring length as possible to reduce the amount of SizeUTF8 calls
						length = 0;
						for (int chunkLength = (int)Math.Pow(10, Math.Floor(Math.Log10(lineRemainder.Length))); chunkLength >= 1; chunkLength = chunkLength / 10)
						{
							w = 0;
							while (w < maxWidth)
							{
								length += chunkLength;

								//would be invalid to insert into upcoming substring call
								if (length > lineRemainder.Length)
									break;

								TTF.SizeUTF8(Handle, lineRemainder.Substring(0, length), out w, out h);
							}

							//revert last addition
							length -= chunkLength;
						}

						//if length still is zero the character would be pushed back till the line limit, which is not desirable
						if (length <= 0)
							break;

						result += lineRemainder.Substring(0, length) + '\n';
						lineRemainder = lineRemainder.Substring(length);

						if (++lines == maxLines)
							break;
					}
				}

				if (lines == maxLines)
					break;
			}

			//last line cannot end with newline
			if (lines == maxLines && result[result.Length - 1] == '\n')
				result = result.Remove(result.Length - 1);

			charsTrimmed += str.Substring(lineStart + lineLength).Length + lineRemainder.Length;

			return result;
		}
		public string WrapString(string str, float maxWidth, int maxLines = int.MaxValue)
		{
			int charsTrimmed;
			return WrapString(str, maxWidth, maxLines, out charsTrimmed);
		}

		public void Load()
		{
			if (!Loaded)
			{
				Handle = TTF.OpenFontRW(Resources.CreateRWFromFile(Filename, FileAssembly), 1, PointSize);

				if (Handle != IntPtr.Zero)
				{
					LineHeight = TTF.FontLineSkip(Handle);
					Loaded = true;
				}
				else
					throw new FileLoadException(SDL.GetError());
			}
		}

		public void Unload()
		{
			if (Loaded)
			{
				TTF.CloseFont(Handle);
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
		Bottom = 32,

		TopLeft = Top | Left,
		BottomRight = Bottom | Right,
		CenterMiddle = Center | Middle
	}
}