using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Audio;
using OpenTK.Graphics;
using OpenTK.Audio.OpenAL;

//OpenTK 1.1.1420.5205
//NVorbis 0.7.4 for decoding ogg sound
//QuickFont 1.0.2 modified and built upon OpenTK 1.1.1420.5205

namespace HatlessEngine
{
	public static class Game
	{
		internal static GameWindow Window;
		internal static AudioContext Audio;
		internal static Rectangle CurrentDrawArea;

		public static bool Running = false;

		/// <summary>
		/// Gets or sets the desired amount of steps per second.
		/// </summary>
		public static float Speed
		{ 
			get { return (float)Window.TargetUpdateFrequency; }
			set { Window.TargetUpdateFrequency = value; }
		}
		/// <summary>
		/// Gets the actual amount of steps per second.
		/// </summary>
		public static float StepsPerSecond
		{
			get { return (float)Window.UpdateFrequency; }
		}

		/// <summary>
		/// Gets the actual amount of frames per second.
		/// </summary>
		public static float FramesPerSecond
		{
			get { return (float)Window.RenderFrequency; }
		}

		public static void Run(float speed = 100f)
		{
			GameWindowFlags flags = GameWindowFlags.Default;
			if (WindowSettings.InitialState == WindowState.Fullscreen)
				flags = GameWindowFlags.Fullscreen;
				
			Window = new GameWindow(WindowSettings.InitialWidth, WindowSettings.InitialHeight, GraphicsMode.Default, WindowSettings.InitialTitle, flags);
			Running = true;

			//apply remaining settings
			if (WindowSettings.InitialPosition != Point.Zero)
				WindowSettings.Position = WindowSettings.InitialPosition;
			WindowSettings.DesiredFrameRate = WindowSettings.InitialDesiredFrameRate;
			WindowSettings.VSync = WindowSettings.InitialVSync;
			WindowSettings.CursorVisible = WindowSettings.InitialCursorVisible;
			WindowSettings.Visible = WindowSettings.InitialVisible;
			WindowSettings.State = WindowSettings.InitialState;
			WindowSettings.Border = WindowSettings.InitialBorder;
			WindowSettings.Icon = WindowSettings.InitialIcon;

			//OpenGL initialization
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Enable(EnableCap.AlphaTest); //sorta hacky fix for glyph transparency
			GL.AlphaFunc(AlphaFunction.Greater, 0f);

			GL.ClearColor((Color4)Color.Gray);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.ClearDepth(1d);
			GL.DepthRange(1d, 0d); //does not seem right, but it works (see it as duct-tape)

			Resources.AddView(new Rectangle(new Point(0f, 0f), WindowSettings.Size), new Rectangle(0f, 0f, 1f, 1f));

			Window.UpdateFrame += Step;
			Window.RenderFrame += Draw;
            Window.Closed += ExitCleanup;

			//input
			Window.Mouse.Move += Input.MouseMove;
			Window.Mouse.ButtonDown += Input.MouseButtonChange;
			Window.Mouse.ButtonUp += Input.MouseButtonChange;
			Window.Mouse.WheelChanged += Input.MouseWheelChange;
			Window.Keyboard.KeyDown += Input.KeyboardKeyDown;
			Window.Keyboard.KeyUp += Input.KeyboardKeyUp;

			//clear all buttons when window loses focus
			Window.FocusedChanged += delegate
			{
				if (!Window.Focused)
					Input.CurrentState.Clear();
			};

			//OpenAL initialization
            AudioSettings.SetPlaybackDevice();

			if (Started != null)
				Started(null, EventArgs.Empty);

			Window.Run(speed);
		}

		private static void Step(object sender, FrameEventArgs e)
		{
			Input.UpdateState();

			//update the weakreferences if they still exist
			Resources.UpdateManagedSprites();

			foreach (LogicalObject obj in Resources.Objects)
			{
				obj.Step();
				obj.AfterStep();
			}

			Resources.SourceRemoval();
			Resources.ObjectAdditionAndRemoval();

			//update input state (push buttonlist) for update before next step
			Input.PushButtons();
		}

		private static void Draw(object sender, FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			//reset depth and color to be consistent over multiple frames
			DrawX.Depth = 0;
			DrawX.DefaultColor = Color.Black;

			foreach(View view in Resources.Views)
			{
				CurrentDrawArea = view.Area;
				GL.Viewport((int)view.Viewport.X * Window.Width, (int)view.Viewport.Y * Window.Height, (int)view.Viewport.X2 * Window.Width, (int)view.Viewport.Y2 * Window.Height);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(view.Area.X, view.Area.X2, view.Area.Y2, view.Area.Y, -1f, 1f);

				GL.MatrixMode(MatrixMode.Modelview);
				//drawing
				foreach (LogicalObject obj in Resources.Objects)
				{
					//set view's coords for clipping?
					obj.Draw();
				}
			}

			GL.Flush();
			Window.Context.SwapBuffers();
		}
		public static void Exit()
		{
			Window.Close();
		}
        /// <summary>
        /// Will be fired after the window is closed (so after exiting).
        /// </summary>
        private static void ExitCleanup(object sender, EventArgs e)
        {
            Log.CloseAllStreams();
        }

		public static event EventHandler Started;
	}
}

