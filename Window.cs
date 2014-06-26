using System;
using System.IO;
using SDL2;

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
			SDL.SDL_GetWindowPosition(Game.WindowHandle, out x, out y);
			return new Point(x, y);
		}
		public static void SetPosition(Point pos)
		{
			SDL.SDL_SetWindowPosition(Game.WindowHandle, (int)pos.X, (int)pos.Y);
		}

		public static Point GetSize()
		{
			int w, h;
			SDL.SDL_GetWindowSize(Game.WindowHandle, out w, out h);
			return new Point(w, h);
		}
		public static void SetSize(Point size)
		{
			SDL.SDL_SetWindowSize(Game.WindowHandle, (int)size.X, (int)size.Y);
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

		//more plz
	}

	public enum ScreenMode
	{
		Windowed = 0,
		Fullscreen = 1,
		FakeFullscreen = 2
	}
}