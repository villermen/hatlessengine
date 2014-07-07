using System;
using System.IO;
using SDL2;
using SDL2_image;

namespace HatlessEngine
{
	/// <summary>
	/// Manages the game window's properties.
	/// </summary>
	public static class Window
	{
		public static Point GetPosition()
		{
			int x, y;
			SDL.GetWindowPosition(Game.WindowHandle, out x, out y);
			return new Point(x, y);
		}
		public static void SetPosition(Point pos)
		{
			SDL.SetWindowPosition(Game.WindowHandle, (int)pos.X, (int)pos.Y);
		}

		public static Point GetSize()
		{
			int w, h;
			SDL.GetWindowSize(Game.WindowHandle, out w, out h);
			return new Point(w, h);
		}
		public static void SetSize(Point size)
		{
			SDL.SetWindowSize(Game.WindowHandle, (int)size.X, (int)size.Y);
		}

		public static string GetTitle()
		{
			return SDL.GetWindowTitle(Game.WindowHandle);
		}
		public static void SetTitle(string title)
		{
			SDL.SetWindowTitle(Game.WindowHandle, title);
		}

		public static bool GetCursorGrab()
		{
			return SDL.GetWindowGrab(Game.WindowHandle) == SDL.Bool.TRUE;
		}
		public static void SetCursorGrab(bool confine)
		{
			SDL.SetWindowGrab(Game.WindowHandle, confine ? SDL.Bool.TRUE : SDL.Bool.FALSE);
		}

		public static ScreenMode GetScreenMode()
		{
			SDL.WindowFlags flags = (SDL.WindowFlags)SDL.GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.WindowFlags.WINDOW_FULLSCREEN))
				return ScreenMode.Fullscreen;
			else if (flags.HasFlag(SDL.WindowFlags.WINDOW_FULLSCREEN_DESKTOP))
				return ScreenMode.FakeFullscreen;
			return ScreenMode.Windowed;
		}
		public static void SetScreenMode(ScreenMode mode)
		{
			uint flag;
			if (mode == ScreenMode.Windowed)
				flag = 0;
			else if (mode == ScreenMode.Fullscreen)
				flag = (uint)SDL.WindowFlags.WINDOW_FULLSCREEN;
			else
				flag = (uint)SDL.WindowFlags.WINDOW_FULLSCREEN_DESKTOP;

			SDL.SetWindowFullscreen(Game.WindowHandle, flag);
		}

		/* CANNOT SET RESIZABLE WITHOUT RECREATING WINDOW BUT ITS NEEDED FOR A FULL BORDERLESS WINDOW SO VERY MEH
		public static bool GetResizable()
		{
			SDL.WindowFlags flags = (SDL.WindowFlags)SDL.GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.WindowFlags.WINDOW_RESIZABLE))
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
			SDL.GetWindowMinimumSize(Game.WindowHandle, out minX, out minY);
			SDL.GetWindowMaximumSize(Game.WindowHandle, out maxX, out maxY);

			return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
		}
		public static void SetResizeLimits(Point minSize, Point maxSize)
		{
			SDL.SetWindowMinimumSize(Game.WindowHandle, (int)minSize.X, (int)minSize.Y);
			SDL.SetWindowMaximumSize(Game.WindowHandle, (int)maxSize.X, (int)maxSize.Y);
		}

		public static bool GetBorder()
		{
			SDL.WindowFlags flags = (SDL.WindowFlags)SDL.GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.WindowFlags.WINDOW_BORDERLESS))
				return false;
			return true;
		}
		public static void SetBorder(bool border)
		{
			SDL.SetWindowBordered(Game.WindowHandle, border ? SDL.Bool.TRUE : SDL.Bool.FALSE);
		}

		public static bool GetVisible()
		{
			SDL.WindowFlags flags = (SDL.WindowFlags)SDL.GetWindowFlags(Game.WindowHandle);
			if (flags.HasFlag(SDL.WindowFlags.WINDOW_SHOWN))
				return true;
			return false;
		}
		public static void SetVisible(bool visible)
		{
			if (visible)
				SDL.ShowWindow(Game.WindowHandle);
			else
				SDL.HideWindow(Game.WindowHandle);
		}

		public static bool GetCursorVisible()
		{
			if (SDL.ShowCursor(-1) == 0)
				return false;
			return true;
		}
		public static void SetCursorVisible(bool visible)
		{
			SDL.ShowCursor(visible ? 1 : 0);
		}

		/// <summary>
		/// Sets the window's icon, it's advised to use a 32x32 image for this (on Windows at least).
		/// </summary>
		public static void SetIcon(string filename)
		{
			SDL.SetWindowIcon(Game.WindowHandle, IMG.Load_RW(Resources.CreateRWFromFile(filename), 1));
		}
		/// <summary>
		/// Will set the window's icon to the default HatlessEngine one.
		/// </summary>
		public static void SetIcon()
		{
			SDL.SetWindowIcon(Game.WindowHandle, IMG.Load_RW(Resources.CreateRWFromFile("defaultwindowicon.png"), 1));
		}

		/// <summary>
		/// Same as calling Cursor.Set()
		/// </summary>
		public static void SetCursor(Cursor cursor)
		{
			cursor.Set();
		}
		public static void SetCursor(string cursorID)
		{
			SetCursor(Resources.Cursors[cursorID]);
		}

		//left to do: cursorimage & maximizing/minimizing
	}

	public enum ScreenMode
	{
		Windowed = 0,
		Fullscreen = 1,
		FakeFullscreen = 2
	}
}