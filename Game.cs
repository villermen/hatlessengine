using System;
using SDL2;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using System.Collections.Generic;
using MoreLinq;
using System.Diagnostics;
using System.Threading;

namespace HatlessEngine
{
	public static class Game
	{
		internal static IntPtr WindowHandle;
		internal static IntPtr RendererHandle;
		internal static AudioContext AudioContext;
		internal static QuadTree QuadTree;

		internal static bool RenderframeReady = false;

		private static bool Running = false;

		private static int TicksPerStep;
		private static int TicksPerDraw;
		private static int _ActualSPS;
		private static int _ActualFPS;

		/// <summary>
		/// Gets or sets the desired amount of steps per second.
		/// </summary>
		public static float StepsPerSecond
		{
			get { return Stopwatch.Frequency / TicksPerStep; }
			set 
			{
				if (value > 0)
					TicksPerStep = (int)(Stopwatch.Frequency / value);
				else
					throw new ArgumentOutOfRangeException("StepsPerSecond", "StepsPerSecond must be positive and nonzero. (" + value.ToString() + " given)");
			}
		}

		/// <summary>
		/// Gets or sets the limit of frames per second.
		/// If 0 no limit is enforced.
		/// </summary>
		public static float FPSLimit
		{
			get 
			{
				if (TicksPerDraw == 0)
					return 0;
				return Stopwatch.Frequency / TicksPerDraw; 
			}
			set
			{
				if (value > 0)
					TicksPerDraw = (int)(Stopwatch.Frequency / value);
				else if (value == 0)
					TicksPerDraw = 0;
				else
					throw new ArgumentOutOfRangeException("FPSLimit", "FPSLimit must be positive or zero. (" + value.ToString() + " given)");
			}
		}

		/// <summary>
		/// Returns the actual number of Steps Per Second, calculated from one step interval.
		/// </summary>
		public static int ActualSPS
		{
			get { return _ActualSPS; }
		}
		/// <summary>
		/// Returns the actual number of Frames Per Second, calculated from one draw interval.
		/// </summary>
		public static int ActualFPS
		{
			get { return _ActualFPS; }
		}

		/// <summary>
		/// Sets up a window and enters the gameloop. (Code after this call won't run until the game has exited.)
		/// </summary>
		public static void Run(float speed = 100f)
		{
			SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
			SDL_ttf.TTF_Init();
			SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);
			SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_TIF);
			SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_WEBP);

			SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_VSYNC, "1");
			SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1");

			WindowHandle = SDL.SDL_CreateWindow("HatlessEngine", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
			RendererHandle = SDL.SDL_CreateRenderer(WindowHandle, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC));
			SDL.SDL_SetRenderDrawBlendMode(RendererHandle, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

			Window.SetIcon();

			//add default view that spans the current window
			new View("default", new SimpleRectangle(Point.Zero, Window.GetSize()), new SimpleRectangle(Point.Zero , new Point(1f, 1f)));

			//OpenAL initialization
			AudioSettings.SetPlaybackDevice();

			StepsPerSecond = speed;

			Running = true;			

			if (Started != null)
				Started(null, EventArgs.Empty);

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			//do a step before the first draw can occur
			Step();

			long lastStepTick = 0;
			long lastDrawTick = 0;
			long lastStepTime = 0;
			long lastDrawTime = 0;

			while (Running)
			{
				//perform step when needed
				if (stopWatch.ElapsedTicks >= lastStepTick + TicksPerStep)
				{
					lastStepTick = lastStepTick + TicksPerStep;
					Step();

					_ActualSPS = (int)(Stopwatch.Frequency / (stopWatch.ElapsedTicks - lastStepTime));
					lastStepTime = stopWatch.ElapsedTicks;
				}

				//perform draw when ready for a new one
				if (!RenderframeReady && Running && stopWatch.ElapsedTicks >= lastDrawTick + TicksPerDraw)
				{
					lastDrawTick = lastDrawTick + TicksPerDraw;
					Draw();

					_ActualFPS = (int)(Stopwatch.Frequency / (stopWatch.ElapsedTicks - lastDrawTime));
					lastDrawTime = stopWatch.ElapsedTicks;
				}
			}

			//cleanup and uninitialization
			Resources.UnloadAllExternalResources();
			Log.CloseAllStreams();
			Input.CloseGamepads();

			SDL.SDL_DestroyWindow(WindowHandle);
			WindowHandle = IntPtr.Zero;
			SDL.SDL_DestroyRenderer(RendererHandle);
			RendererHandle = IntPtr.Zero;
			AudioContext.Dispose();
			AudioContext = null;

			SDL.SDL_Quit();
			SDL_ttf.TTF_Quit();
			SDL_image.IMG_Quit();
		}

		private static void Step()
		{
			//push input state
			Input.PushState();

			//process all SDL events
			SDL.SDL_Event e;
			while (SDL.SDL_PollEvent(out e) == 1)
			{
				switch (e.type)
				{
					//let Input handle input related events
					case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
					case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
					case SDL.SDL_EventType.SDL_MOUSEWHEEL:
					case SDL.SDL_EventType.SDL_MOUSEMOTION:
					case SDL.SDL_EventType.SDL_KEYDOWN:
					case SDL.SDL_EventType.SDL_KEYUP:
					case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
					case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:					
					case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
					case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
					case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:					
						Input.InputEvent(e);
						break;
					
					//global quit, not only the window's exit button
					case SDL.SDL_EventType.SDL_QUIT:
						Exit();
						break;					
				}
			}

			Input.ApplyButtonMaps();

			//update the weakreferences if they still exist
			Resources.UpdateManagedSprites();

			foreach (LogicalObject obj in Resources.Objects)
			{
				if (!obj.Destroyed)
				obj.Step();
			}

			//collision time!
			float minX = float.PositiveInfinity;
			float minY = float.PositiveInfinity;
			float maxX = float.NegativeInfinity;
			float maxY = float.NegativeInfinity;
			foreach(PhysicalObject obj in Resources.PhysicalObjects)
			{
				obj.UpdateCoverableArea();

				if (obj.CoverableArea.Position1.X < minX)
					minX = obj.CoverableArea.Position1.X;
				if (obj.CoverableArea.Position2.X > maxX)
					maxX = obj.CoverableArea.Position2.X;
				if (obj.CoverableArea.Position1.Y < minY)
					minY = obj.CoverableArea.Position1.Y;
				if (obj.CoverableArea.Position2.Y > maxY)
					maxY = obj.CoverableArea.Position2.Y;

				//set before the actual collision check phase
				obj.SpeedLeft = 1f;
				obj.CollisionCandidates = null;
			}
			
			//create and fill quadtree for this step
			QuadTree = new QuadTree(new SimpleRectangle(minX, minY, maxX - minX, maxY - minY));

			//maybe if too slow use SortedList and compare using the default comparer
			List<PhysicalObject> processingObjects = new List<PhysicalObject>(Resources.PhysicalObjects);	   
		 
			//get all first collision speedfractions for all objects and sort the list
			foreach (PhysicalObject obj in processingObjects)
				obj.CalculateClosestCollision();

			while (processingObjects.Count > 0)			
			{
				//get closest collision, process it/the pair of objects
				PhysicalObject obj = processingObjects.MinBy(MovementForPhysicalObject);

				obj.PerformClosestCollision();

				//remove/recalculate collisions
				if (obj.SpeedLeft == 0f)
					processingObjects.Remove(obj);
				else
					obj.CalculateClosestCollision();

				//recalculate for all possibly influenced objects (if needed)
				if (obj.CollisionCandidates != null)
				{
					foreach (PhysicalObject influencedObj in obj.CollisionCandidates)
						influencedObj.CalculateClosestCollision();
				}
			}

			Resources.SourceRemoval();
			Resources.ObjectAdditionAndRemoval();
			Resources.CleanupFontTextures();
		}

		private static float MovementForPhysicalObject(PhysicalObject obj)
		{
			return obj.ClosestCollisionSpeedFraction * obj.SpeedVelocity;
		}

		private static void Draw()
		{
			//collect drawjobs
			foreach (LogicalObject obj in Resources.Objects)
			{
				if (!obj.Destroyed)
					obj.Draw();
			}

			DrawX.DrawJobs.Sort((j1, j2) => -j1.Depth.CompareTo(j2.Depth));

			SDL.SDL_SetRenderDrawColor(RendererHandle, DrawX.BackgroundColor.R, DrawX.BackgroundColor.G, DrawX.BackgroundColor.B, DrawX.BackgroundColor.A);
			SDL.SDL_RenderClear(RendererHandle);

			Point windowSize = Window.GetSize();

			foreach (View view in Resources.Views.Values)
			{
				if (!view.Active)
					continue;

				Point scale = view.Viewport.Size * windowSize / view.Area.Size;
				SDL.SDL_RenderSetScale(RendererHandle, scale.X, scale.Y);

				//viewport is affected by scale for whatever reason, correct it
				SDL.SDL_Rect viewport = (SDL.SDL_Rect)new SimpleRectangle(view.Viewport.Position1 * windowSize / scale, view.Viewport.Size * windowSize / scale);
				SDL.SDL_RenderSetViewport(RendererHandle, ref viewport);

				//get all jobs that will draw inside this view
				foreach (IDrawJob job in DrawX.GetDrawJobsByArea(view.Area))
				{
					if (job.Type == DrawJobType.Texture) //draw a texture
					{
						TextureDrawJob textureDrawJob = (TextureDrawJob)job;
						SDL.SDL_Rect sourceRect = (SDL.SDL_Rect)textureDrawJob.SourceRect;
						SDL.SDL_Rect destRect = (SDL.SDL_Rect)new SimpleRectangle(textureDrawJob.DestRect.Position - view.Area.Position1 - textureDrawJob.DestRect.Origin, textureDrawJob.DestRect.Size);

						if (textureDrawJob.DestRect.Rotation == 0f)
						{
							SDL.SDL_RenderCopy(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect);
						}
						else
						{
							SDL.SDL_Point origin = (SDL.SDL_Point)textureDrawJob.DestRect.Origin;
							SDL.SDL_RenderCopyEx(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect, textureDrawJob.DestRect.Rotation, ref origin, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
						}
					}
					else //draw line(s)
					{
						LineDrawJob lineDrawJob = (LineDrawJob)job;

						//transform all points according to view and cast em
						SDL.SDL_Point[] sdlPoints = Array.ConvertAll<Point, SDL.SDL_Point>(lineDrawJob.Points, p => (SDL.SDL_Point)(p - view.Area.Position1));

						SDL.SDL_SetRenderDrawColor(RendererHandle, lineDrawJob.Color.R, lineDrawJob.Color.G, lineDrawJob.Color.B, lineDrawJob.Color.A);
						SDL.SDL_RenderDrawLines(RendererHandle, sdlPoints, lineDrawJob.PointCount);
					}
				}
			}

			DrawX.DrawJobs.Clear();

			//threadpool should take care of actually swapping the frames (RenderPresent may wait for things like Fraps or VSync)
			RenderframeReady = true;
			Stopwatch v = Stopwatch.StartNew();
			ThreadPool.QueueUserWorkItem(new WaitCallback(PresentRender), v);
		}

		/// <summary>
		/// Game will finish it's current loop (Step or Draw) and exit the Running method.
		/// </summary>
		public static void Exit()
		{
			Running = false;
		}
		
		public static event EventHandler Started;

		private static void PresentRender(object o)
		{
			SDL.SDL_RenderPresent(RendererHandle);
			RenderframeReady = false;
		}
	}
}