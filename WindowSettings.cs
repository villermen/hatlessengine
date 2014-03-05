using System;

namespace HatlessEngine
{
	/// <summary>
	/// For getting and setting window/graphics properties.
	/// If the game is not running, this changes the initial state the window will have.
	/// </summary>
	public static class WindowSettings
	{
		internal static Point InitialPosition = Point.Zero;
		internal static int InitialWidth = 800;
		internal static int InitialHeight = 600;
		internal static float InitialDesiredFrameRate = 60f;
		internal static bool InitialVSync = false;
		private static bool _AdaptiveVSync = false;
		internal static bool InitialCursorVisible = true;
		internal static string InitialTitle = "HatlessEngine";
		internal static bool InitialVisible = true;
		internal static WindowState InitialState = WindowState.Normal;
		internal static bool InitialBorder = true;
		private static bool _Resizable = true;

		public static Point Position
		{
			get 
			{ 
				if (Game.Running)
					return new Point(Game.Window.Location.X, Game.Window.Location.Y);
				else
					return InitialPosition;
			}
			set 
			{ 
				if (Game.Running)
					Game.Window.Location = new System.Drawing.Point((int)value.X, (int)value.Y);
				else
					InitialPosition = value;
			}
		}
		public static Point Size
		{
			get 
			{ 
				if (Game.Running)
					return new Point(Game.Window.ClientSize.Width, Game.Window.ClientSize.Height);
				else
					return new Point(InitialWidth, InitialHeight);
			}
			set 
			{ 
				if (Game.Running)
				{
					Game.Window.Width = (int)value.X;
					Game.Window.Height = (int)value.Y;
				}
				else
				{
					InitialWidth = (int)value.X;
					InitialHeight = (int)value.Y;
				}
			}
		}
		public static float DesiredFrameRate
		{
			get 
			{ 
				if (Game.Running)
					return (float)Game.Window.TargetRenderFrequency;
				else
					return InitialDesiredFrameRate;
			}
			set 
			{
				if (Game.Running)
					Game.Window.TargetRenderFrequency = value;
				InitialDesiredFrameRate = value; 
			}
		}
		public static bool VSync
		{
			get 
			{
				if (Game.Running)
					return Game.Window.VSync == OpenTK.VSyncMode.On || Game.Window.VSync == OpenTK.VSyncMode.Adaptive;
				else
					return InitialVSync;
			}
			set 
			{ 
				if (Game.Running)
				{
					if (value)
					{
						if (_AdaptiveVSync)
							Game.Window.VSync = OpenTK.VSyncMode.Adaptive;
						else
							Game.Window.VSync = OpenTK.VSyncMode.On;
					}
					else
						Game.Window.VSync = OpenTK.VSyncMode.Off;
				}
				else
					InitialVSync = value; 
			}
		}
		public static bool AdaptiveVSync
		{
			get { return _AdaptiveVSync; }
			set 
			{ 
				if (Game.Running && VSync)
				{
					if (value)
						Game.Window.VSync = OpenTK.VSyncMode.Adaptive;
					else
						Game.Window.VSync = OpenTK.VSyncMode.On;
				}
				_AdaptiveVSync = value;
			}
		}
		public static bool CursorVisible
		{
			get 
			{ 
				if (Game.Running)
					return Game.Window.CursorVisible;
				else
					return InitialCursorVisible;
			}
			set 
			{
				if (Game.Running)
					Game.Window.CursorVisible = value;
				else
					InitialCursorVisible = value; 
			}
		}
		public static string Title
		{
			get 
			{ 
				if (Game.Running)
					return Game.Window.Title;
				else
					return InitialTitle;
			}
			set 
			{ 
				if (Game.Running)
					Game.Window.Title = value;
				else
					InitialTitle = value; 
			}
		}
		public static bool Visible
		{
			get 
			{ 
				if (Game.Running)
					return Game.Window.Visible;
				else
					return InitialVisible;
			}
			set 
			{ 
				if (Game.Running)
					Game.Window.Visible = value;
				else
					InitialVisible = value; 
			}
		}
		public static WindowState State
		{
			get 
			{
				if (Game.Running)
					return (WindowState)Game.Window.WindowState;
				else
					return InitialState;
			}
			set 
			{ 
				if (Game.Running)
					Game.Window.WindowState = (OpenTK.WindowState)value;
				else
					InitialState = value;
			}
		}
		public static bool Border
		{
			get 
			{ 
				if (Game.Running)
					return Game.Window.WindowBorder == OpenTK.WindowBorder.Resizable || Game.Window.WindowBorder == OpenTK.WindowBorder.Fixed;
				else
					return InitialBorder;
			}
			set
			{
				if (Game.Running)
				{
					if (value)
					{
						if (_Resizable)
							Game.Window.WindowBorder = OpenTK.WindowBorder.Resizable;
						else
							Game.Window.WindowBorder = OpenTK.WindowBorder.Fixed;
					}
					else
						Game.Window.WindowBorder = OpenTK.WindowBorder.Hidden;
				}
				else
					InitialBorder = value; 
			}
		}
		public static bool Resizable
		{
			get { return _Resizable; }
			set 
			{
				if (Game.Running && Border)
				{
					if (value)
						Game.Window.WindowBorder = OpenTK.WindowBorder.Resizable;
					else
						Game.Window.WindowBorder = OpenTK.WindowBorder.Fixed;
				}
				_Resizable = value;
			}
		}

		//Icon
		//cursor
		//border buttons
	}

	public enum WindowState
	{
		Normal = 0,
		Minimized = 1,
		Maximized = 2,
		Fullscreen = 3
	}
}

