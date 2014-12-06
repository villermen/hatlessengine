using System;
using System.IO;
using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Class containing a cursor image and an origin offset that will be clicked at.
	/// </summary>
	public class Cursor : IExternalResource
	{
		public string Id { get; private set; }
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public bool Loaded { get; private set; }

		private readonly bool _isSystemCursor;
		private readonly SystemCursor _systemCursor;
		private IntPtr _cursorHandle;

		public Point Origin;

		private Cursor(string id)
		{
			Id = id;
			Loaded = false;

			Resources.Cursors.Add(Id, this);
			Resources.ExternalResources.Add(this);
		}
		/// <summary>
		/// Creates a cursor from an image file.
		/// </summary>
		public Cursor(string id, string filename, Point origin)
			: this(id)
		{
			Filename = filename;
			FileAssembly = Assembly.GetCallingAssembly();
			Origin = origin;
			_isSystemCursor = false;	
		}
		/// <summary>
		/// Creates a system cursor.
		/// </summary>
		public Cursor(string id, SystemCursor cursor)
			: this(id)
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

		public void Load()
		{
			if (Loaded)
				return;

			if (_isSystemCursor)
				_cursorHandle = SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)_systemCursor);
			else
				_cursorHandle = SDL.SDL_CreateColorCursor(SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(Filename, FileAssembly), 1), (int)Origin.X, (int)Origin.Y);
			
			if (_cursorHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public void Unload()
		{
			if (!Loaded)
				return;

			SDL.SDL_FreeCursor(_cursorHandle);
			_cursorHandle = IntPtr.Zero;
			Loaded = false;

		}

		public void Destroy()
		{
			Unload();

			Resources.Cursors.Remove(Id);
			Resources.ExternalResources.Remove(this);
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

		~Cursor()
		{
			Dispose(false);
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
