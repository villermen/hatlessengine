using System;
using System.IO;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Class containing a cursor image and an origin offset that will be clicked at.
	/// </summary>
	public class Cursor : ExternalResource
	{
		private readonly bool _isSystemCursor;
		private readonly SystemCursor _systemCursor;
		private IntPtr _cursorHandle;

		public Point Origin;

		//TODO: filename is discarded here, improve on this
		private Cursor(string id, string file)
			: base(id, file) { }

		/// <summary>
		/// Creates a cursor from an image file.
		/// </summary>
		public Cursor(string id, string file, Point origin)
			: this(id, file)
		{
			Origin = origin;
			_isSystemCursor = false;	
		}

		/// <summary>
		/// Creates a system cursor.
		/// </summary>
		public Cursor(string id, SystemCursor cursor)
			: this(id, "")
		{
			_isSystemCursor = true;
			_systemCursor = cursor;
		}

		public void Set()
		{
			if (Loaded)
				SDL.SDL_SetCursor(_cursorHandle);
			else
				throw new NotLoadedException();
		}

		public override void Load()
		{
			if (Loaded)
				return;

			if (_isSystemCursor)
				_cursorHandle = SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)_systemCursor);
			else
				_cursorHandle = SDL.SDL_CreateColorCursor(SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(File, FileAssembly), 1), (int)Origin.X, (int)Origin.Y);
			
			if (_cursorHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public override void Unload()
		{
			if (!Loaded)
				return;

			SDL.SDL_FreeCursor(_cursorHandle);
			_cursorHandle = IntPtr.Zero;
			Loaded = false;
		}

		public static implicit operator Cursor(string id)
		{
			return Resources.Get<Cursor>(id);
		}
	}

	public enum SystemCursor
	{
		Arrow = 0,
		Beam = 1,
		Wait = 2,
		Crosshair = 3,
		WaitArrow = 4,
		SizeNorthWestAndSouthEast = 5,
		SizeNorthEastAndSouthWest = 6,
		SizeWestAndEast = 7,
		SizeNorthAndSouth = 8,
		SizeAll = 9,
		Invalid = 10,
		Hand = 11
	}
}
