using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Audio;
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

		public static float Speed
		{ 
			get { return (float)Window.TargetUpdateFrequency; }
			set { Window.TargetUpdateFrequency = value; }
		}
		public static float LPS
		{
			get { return (float)Window.UpdateFrequency; }
		}
		public static float FPS
		{
			get { return (float)Window.RenderFrequency; }
		}

		public static void Run(Size windowSize, float speed = 100)
		{
			Window = new GameWindow(windowSize.Width, windowSize.Height);

			//OpenGL initialization
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.ClearColor(Color.Gray);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.ClearDepth(1f);
			GL.DepthRange(1d, 0d); //does not seem right, but it works (see it as duct-tape)

			Resources.AddView(new RectangleF(new PointF(0, 0), windowSize), new RectangleF(0, 0, 1, 1));

			Window.UpdateFrame += Step;
			Window.RenderFrame += Draw;

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
			Audio = new AudioContext();

			if (Started != null)
				Started(null, EventArgs.Empty);

			Window.Run(speed);
		}

		private static void Step(object sender, FrameEventArgs e)
		{
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
			Input.UpdateState();
		}

		private static void Draw(object sender, FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			//reset depth and color to be consistent over multiple frames
			DrawX.Depth = 0;
			DrawX.DefaultColor = Color.Black;

			foreach(View view in Resources.Views)
			{
				GL.Viewport((int)view.Viewport.Left * Window.Width, (int)view.Viewport.Top * Window.Height, (int)view.Viewport.Right * Window.Width, (int)view.Viewport.Bottom * Window.Height);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(view.Area.Left, view.Area.Right, view.Area.Bottom, view.Area.Top, -1f, 1f);

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

		public static event EventHandler Started;
	}
}

