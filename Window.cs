using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Manages the game window's properties.
	/// </summary>
	public static class Window
	{
		private static Point _size = new Point(800f, 600f);

		public static Point Size
		{
			get { return _size; }
		}

		public static void SetSize(Point size)
		{
			SDL.SDL_SetWindowSize(Game.WindowHandle, (int)size.X, (int)size.Y);
		}

		public static Point GetPosition()
		{
			int x, y;
			SDL.SDL_GetWindowPosition(Game.WindowHandle, out x, out y);
			return new Point(x, y);
		}
		public static void SetPosition(Point pos)
		{
			SDL.SDL_SetWindowPosition(Game.WindowHandle, (int)pos.X, (int)pos.Y);
		}

		public static string GetTitle()
		{
			return SDL.SDL_GetWindowTitle(Game.WindowHandle);
		}
		public static void SetTitle(string title)
		{
			SDL.SDL_SetWindowTitle(Game.WindowHandle, title);
		}

		public static bool GetCursorGrab()
		{
			return SDL.SDL_GetWindowGrab(Game.WindowHandle) == SDL.SDL_bool.SDL_TRUE;
		}
		public static void SetCursorGrab(bool confine)
		{
			SDL.SDL_SetWindowGrab(Game.WindowHandle, confine ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
		}

		public static ScreenMode GetScreenMode()
		{
			SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN))
				return ScreenMode.Fullscreen;
			
			if (flags.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP))
				return ScreenMode.FakeFullscreen;

			return ScreenMode.Windowed;
		}
		public static void SetScreenMode(ScreenMode mode)
		{
			uint flag;
			if (mode == ScreenMode.Windowed)
				flag = 0;
			else if (mode == ScreenMode.Fullscreen)
				flag = (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			else
				flag = (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;

			SDL.SDL_SetWindowFullscreen(Game.WindowHandle, flag);
		}

		/* CANNOT SET RESIZABLE WITHOUT RECREATING WINDOW BUT ITS NEEDED FOR A FULL BORDERLESS WINDOW SO VERY MEH
		public static bool GetResizable()
		{
			SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.SDL_WindowFlags.WINDOW_RESIZABLE))
				return true;
			return false;
		}
		public static void SetResizable(bool resizable)
		{
			throw new NotImplementedException();
		}*/

		public static Point[] GetResizeLimits()
		{
			int minX, minY, maxX, maxY;
			SDL.SDL_GetWindowMinimumSize(Game.WindowHandle, out minX, out minY);
			SDL.SDL_GetWindowMaximumSize(Game.WindowHandle, out maxX, out maxY);

			return new [] { new Point(minX, minY), new Point(maxX, maxY) };
		}
		public static void SetResizeLimits(Point minSize, Point maxSize)
		{
			SDL.SDL_SetWindowMinimumSize(Game.WindowHandle, (int)minSize.X, (int)minSize.Y);
			SDL.SDL_SetWindowMaximumSize(Game.WindowHandle, (int)maxSize.X, (int)maxSize.Y);
		}

		public static bool GetBorder()
		{
			SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS))
				return false;
			return true;
		}
		public static void SetBorder(bool border)
		{
			SDL.SDL_SetWindowBordered(Game.WindowHandle, border ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
		}

		public static bool GetVisible()
		{
			SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN))
				return true;
			return false;
		}
		public static void SetVisible(bool visible)
		{
			if (visible)
				SDL.SDL_ShowWindow(Game.WindowHandle);
			else
				SDL.SDL_HideWindow(Game.WindowHandle);
		}

		public static bool GetCursorVisible()
		{
			if (SDL.SDL_ShowCursor(-1) == 0)
				return false;
			return true;
		}
		public static void SetCursorVisible(bool visible)
		{
			SDL.SDL_ShowCursor(visible ? 1 : 0);
		}

		/// <summary>
		/// Sets the window's icon, it's advised to use a 32x32 image for this (on Windows at least).
		/// </summary>
		public static void SetIcon(string filename)
		{
			SDL.SDL_SetWindowIcon(Game.WindowHandle, SDL_image.IMG_Load_RW(Resources.CreateRWFromFile(filename, Assembly.GetCallingAssembly()), 1));
		}
		/// <summary>
		/// Will set the window's icon to the default HatlessEngine one.
		/// </summary>
		public static void SetIcon()
		{
			SDL.SDL_SetWindowIcon(Game.WindowHandle, SDL_image.IMG_Load_RW(Resources.CreateRWFromFile("defaultwindowicon.png", Assembly.GetCallingAssembly()), 1));
		}

		/// <summary>
		/// Same as calling Cursor.Set()
		/// </summary>
		public static void SetCursor(Cursor cursor)
		{
			cursor.Set();
		}

		//left to do: maximizing/minimizing

		/// <summary>
		/// Handles all window related SDL.SDL_events.
		/// </summary>
		internal static void WindowEvent(SDL.SDL_Event e)
		{
			switch (e.window.windowEvent)
			{
				case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
					_size = new Point(e.window.data1, e.window.data2);
					break;
			}
		}

	}

	public enum ScreenMode
	{
		Windowed = 0,
		Fullscreen = 1,
		FakeFullscreen = 2
	}
}