using MoreLinq;
using SDL2;
using SDL2_image;
using SDL2_mixer;
using SDL2_ttf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace HatlessEngine
{
	public static class Game
	{
		internal static IntPtr WindowHandle;
		internal static IntPtr RendererHandle;
		internal static QuadTree QuadTree;

		internal static bool RenderframeReady = false;

		private static bool Running = false;

		private static int TicksPerStep;
		private static int TicksPerDraw;
		private static int _ActualSPS;
		private static int _ActualFPS;

		/// <summary>
		/// If true the game will force to run steps at max frequency until it has caught up with the required speed.
		/// If not it will just keep going at regular speed after it goes out of sync.
		/// </summary>
		public static bool CatchUpSteps = false;

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

		static Game()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
		}

		/// <summary>
		/// Sets up a window and enters the gameloop. (Code after this call won't run until the game has exited.)
		/// </summary>
		public static void Run(float speed = 100f)
		{
			SDL.Init(SDL.InitFlags.EVERYTHING);
			IMG.Init(IMG.InitFlags.EVERYTHING);
			Mix.Init(Mix.InitFlags.EVERYTHING);
			TTF.Init();

			//open window
			SDL.SetHint(SDL.HINT_RENDER_VSYNC, "1");
			SDL.SetHint(SDL.HINT_RENDER_SCALE_QUALITY, "1");		

			WindowHandle = SDL.CreateWindow("HatlessEngine", SDL.WINDOWPOS_UNDEFINED, SDL.WINDOWPOS_UNDEFINED, 800, 600, SDL.WindowFlags.WINDOW_SHOWN | SDL.WindowFlags.WINDOW_RESIZABLE | SDL.WindowFlags.WINDOW_INPUT_FOCUS);
			RendererHandle = SDL.CreateRenderer(WindowHandle, -1, (uint)(SDL.RendererFlags.RENDERER_ACCELERATED | SDL.RendererFlags.RENDERER_PRESENTVSYNC));
			SDL.SetRenderDrawBlendMode(RendererHandle, SDL.BlendMode.BLENDMODE_BLEND);

			Window.SetIcon();

			//add default view that spans the current window
			new View("default", new Rectangle(Point.Zero, Window.Size), new Rectangle(Point.Zero , new Point(1f, 1f)));

			//initialize audio system and let Resources handle sound expiration
			Mix.OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 4096);
			Mix.AllocateChannels((int)(speed * 2)); //might want to dynamically create and remove channels during runtime
			Mix.ChannelFinished(new Mix.ChannelFinishedDelegate(Resources.SoundChannelFinished));
			Mix.HookMusicFinished(new Mix.MusicFinishedDelegate(Resources.MusicFinished));

			//initialize loop
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
					if (CatchUpSteps)
						lastStepTick = lastStepTick + TicksPerStep;
					else
						lastStepTick = stopWatch.ElapsedTicks;
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

			SDL.DestroyWindow(WindowHandle);
			WindowHandle = IntPtr.Zero;
			SDL.DestroyRenderer(RendererHandle);
			RendererHandle = IntPtr.Zero;

			Mix.CloseAudio();

			SDL.Quit();
			IMG.Quit();
			Mix.Quit();
			TTF.Quit();
		}

		private static void Step()
		{
			//push input state
			Input.PushState();

			//process all SDL events
			SDL.Event e;
			while (SDL.PollEvent(out e) == 1)
			{
				switch (e.type)
				{
					//let Input handle input related events
					case SDL.EventType.MOUSEBUTTONDOWN:
					case SDL.EventType.MOUSEBUTTONUP:
					case SDL.EventType.MOUSEWHEEL:
					case SDL.EventType.MOUSEMOTION:
					case SDL.EventType.KEYDOWN:
					case SDL.EventType.KEYUP:
					case SDL.EventType.CONTROLLERDEVICEADDED:
					case SDL.EventType.CONTROLLERDEVICEREMOVED:					
					case SDL.EventType.CONTROLLERBUTTONDOWN:
					case SDL.EventType.CONTROLLERBUTTONUP:
					case SDL.EventType.CONTROLLERAXISMOTION:					
						Input.InputEvent(e);
						break;
					
					//let Window handle window related events
					case SDL.EventType.WINDOWEVENT:
						Window.WindowEvent(e);
						break;

					//global quit, not only the window's exit button
					case SDL.EventType.QUIT:
						Exit();
						break;					
				}
			}

			Input.ApplyButtonMaps();

			//update the weakreferences if they still exist
			Resources.UpdateManagedSprites();

			Resources.UpdateViewAreas();

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

				if (obj.CoverableArea.Position.X < minX)
					minX = obj.CoverableArea.Position.X;
				if (obj.CoverableArea.Position2.X > maxX)
					maxX = obj.CoverableArea.Position2.X;
				if (obj.CoverableArea.Position.Y < minY)
					minY = obj.CoverableArea.Position.Y;
				if (obj.CoverableArea.Position2.Y > maxY)
					maxY = obj.CoverableArea.Position2.Y;

				//set before the actual collision check phase
				obj.SpeedLeft = 1f;
				obj.CollisionCandidates = null;
			}

			//create and fill quadtree for this step
			QuadTree = new QuadTree(new Rectangle(minX, minY, maxX - minX, maxY - minY));

			//create list of objects to process and calculate all first collision speedfractions for those objects
			List<PhysicalObject> processingObjects = new List<PhysicalObject>(Resources.PhysicalObjects);	
			foreach (PhysicalObject obj in Resources.PhysicalObjects)
			{
				if (obj.Speed == Point.Zero)
					continue;

				processingObjects.Add(obj);
				obj.CalculateClosestCollision();
			}

			while (processingObjects.Count > 0)			
			{
				//get closest collision, process it/the pair of objects
				PhysicalObject obj = processingObjects.MinBy(o => o.ClosestCollisionSpeedFraction + 1 - o.SpeedLeft);

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

			Resources.ObjectAdditionAndRemoval();
			Resources.CleanupFontTextures();
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

			SDL.SetRenderDrawColor(RendererHandle, DrawX.BackgroundColor.R, DrawX.BackgroundColor.G, DrawX.BackgroundColor.B, DrawX.BackgroundColor.A);
			SDL.RenderClear(RendererHandle);

			foreach (View view in Resources.Views.Values)
			{
				if (!view.Active)
					continue;

				Point scale = view.Viewport.Size * Window.Size / view.Area.Size;

				SDL.RenderSetScale(RendererHandle, scale.X, scale.Y);

				//viewport is affected by scale for whatever reason, correct it
				SDL.Rect viewport = (SDL.Rect)new Rectangle(view.Viewport.Position * Window.Size / scale, view.Viewport.Size * Window.Size / scale);
				SDL.RenderSetViewport(RendererHandle, ref viewport);

				//get all jobs that will draw inside this view
				foreach (IDrawJob job in DrawX.GetDrawJobsByArea(view.Area))
				{
					Type jobType = job.GetType();

					if (jobType == typeof(TextureDrawJob)) //draw a texture
					{
						TextureDrawJob textureDrawJob = (TextureDrawJob)job;
						SDL.Rect sourceRect = (SDL.Rect)textureDrawJob.SourceRect;
						SDL.Rect destRect = (SDL.Rect)new Rectangle(textureDrawJob.DestRect.Position - view.Area.Position - textureDrawJob.DestRect.Origin, textureDrawJob.DestRect.Size);

						if (textureDrawJob.DestRect.Rotation == 0f)
						{
							SDL.RenderCopy(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect);
						}
						else
						{
							SDL.Point origin = (SDL.Point)textureDrawJob.DestRect.Origin;
							SDL.RenderCopyEx(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect, textureDrawJob.DestRect.Rotation, ref origin, SDL.RendererFlip.FLIP_NONE);
						}
					}
					else if (jobType == typeof(LineDrawJob)) //draw line(s)
					{
						LineDrawJob lineDrawJob = (LineDrawJob)job;

						//transform all points according to view and cast em
						SDL.Point[] sdlPoints = Array.ConvertAll<Point, SDL.Point>(lineDrawJob.Points, p => (SDL.Point)(p - view.Area.Position));

						SDL.SetRenderDrawColor(RendererHandle, lineDrawJob.Color.R, lineDrawJob.Color.G, lineDrawJob.Color.B, lineDrawJob.Color.A);
						SDL.RenderDrawLines(RendererHandle, sdlPoints, lineDrawJob.PointCount);
					}
					else //draw filledrect
					{
						FilledRectDrawJob rectDrawJob = (FilledRectDrawJob)job;

						SDL.Rect rect = (SDL.Rect)rectDrawJob.Area;

						SDL.SetRenderDrawColor(RendererHandle, rectDrawJob.Color.R, rectDrawJob.Color.G, rectDrawJob.Color.B, rectDrawJob.Color.A);
						SDL.RenderFillRect(RendererHandle, ref rect);
					}
				}
			}

			DrawX.DrawJobs.Clear();

			//threadpool should take care of actually swapping the frames (RenderPresent may wait for things like Fraps or VSync)
			RenderframeReady = true;
			ThreadPool.QueueUserWorkItem(new WaitCallback(PresentRender));
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
			SDL.RenderPresent(RendererHandle);
			RenderframeReady = false;
		}

		private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;
			string eTypeString = e.GetType().ToString();

			StreamWriter writer = new StreamWriter(File.Open("latestcrashlog.txt", FileMode.Create, FileAccess.Write, FileShare.Read));
			writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm zzz"));
			writer.WriteLine(eTypeString);
			writer.WriteLine(e.Message);
			writer.WriteLine(e.StackTrace);
			writer.Close();

			if (Misc.ExceptionErrorMessageEnabled)
			{
				string message = "The game encountered an unhandled " + eTypeString + " and has to exit.";
				message += "\nA crashlog has been written to latestcrashlog.txt.";

				SDL.ShowSimpleMessageBox(SDL.MessageBoxFlags.MESSAGEBOX_ERROR, "Game Crash", message, WindowHandle);
			}
		}
	}
}