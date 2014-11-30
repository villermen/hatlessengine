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
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public bool Loaded { get; private set; }

		private bool IsSystemCursor;
		private SystemCursor SystemCursor;
		private IntPtr CursorHandle;

		public Point Origin;

		private Cursor(string id)
		{
			ID = id;
			Loaded = false;

			Resources.Cursors.Add(ID, this);
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
			IsSystemCursor = false;	
		}
		/// <summary>
		/// Creates a system cursor.
		/// </summary>
		public Cursor(string id, SystemCursor cursor)
			: this(id)
		{
			IsSystemCursor = true;
			SystemCursor = cursor;
		}

		public void Set()
		{
			if (Loaded)
				SDL.SDL_SetCursor(CursorHandle);
			else
				throw new NotLoadedException();
		}

		public void Load()
		{
			if (Loaded)
				return;

			if (IsSystemCursor)
				CursorHandle = SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)SystemCursor);
			else
				CursorHandle = SDL.SDL_CreateColorCursor(SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(Filename, FileAssembly), 1), (int)Origin.X, (int)Origin.Y);
			
			if (CursorHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public void Unload()
		{
			if (!Loaded)
				return;

			SDL.SDL_FreeCursor(CursorHandle);
			CursorHandle = IntPtr.Zero;
			Loaded = false;

		}

		public void Destroy()
		{
			Unload();

			Resources.Cursors.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}
	}

	public enum SystemCursor
	{
		Arrow = 0,
		IBeam = 1,
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
