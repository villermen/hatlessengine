using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MoreLinq;
using SDL2;

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
			SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

			SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_JPG
				| SDL_image.IMG_InitFlags.IMG_INIT_PNG
				| SDL_image.IMG_InitFlags.IMG_INIT_WEBP
				| SDL_image.IMG_InitFlags.IMG_INIT_TIF);

			SDL_mixer.Mix_Init(SDL_mixer.MIX_InitFlags.MIX_INIT_OGG
				| SDL_mixer.MIX_InitFlags.MIX_INIT_MP3
				| SDL_mixer.MIX_InitFlags.MIX_INIT_FLAC
				| SDL_mixer.MIX_InitFlags.MIX_INIT_MOD
				| SDL_mixer.MIX_InitFlags.MIX_INIT_FLUIDSYNTH);

			SDL_ttf.TTF_Init();

			//open window
			SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_VSYNC, "1");
			SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "2");

			WindowHandle = SDL.SDL_CreateWindow("HatlessEngine", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
			RendererHandle = SDL.SDL_CreateRenderer(WindowHandle, -1, (uint)(SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC));
			SDL.SDL_SetRenderDrawBlendMode(RendererHandle, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

			Window.SetIcon();

			//add default view that spans the current window
			new View("default", new Rectangle(Point.Zero, Window.Size), new Rectangle(Point.Zero , new Point(1f, 1f)));

			//initialize audio system and let Resources handle sound expiration
			SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 4096);
			SDL_mixer.Mix_AllocateChannels((int)(speed * 2)); //might want to dynamically create and remove channels during runtime
			SDL_mixer.Mix_ChannelFinished(new SDL_mixer.ChannelFinishedDelegate(Resources.SoundChannelFinished));
			SDL_mixer.Mix_HookMusicFinished(new SDL_mixer.MusicFinishedDelegate(Resources.MusicFinished));

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

			SDL.SDL_DestroyWindow(WindowHandle);
			WindowHandle = IntPtr.Zero;
			SDL.SDL_DestroyRenderer(RendererHandle);
			RendererHandle = IntPtr.Zero;

			SDL_mixer.Mix_CloseAudio();

			SDL.SDL_Quit();
			SDL_image.IMG_Quit();
			SDL_mixer.Mix_Quit();
			SDL_ttf.TTF_Quit();
		}

		private static void Step()
		{
			//push input state
			Input.PushState();

			//process all SDL.SDL_events
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
					case SDL.SDL_EventType.SDL_TEXTINPUT:
					case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
					case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
					case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
					case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
					case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
						Input.InputEvent(e);
						break;
					
					//let Window handle window related events
					case SDL.SDL_EventType.SDL_WINDOWEVENT:
						Window.WindowEvent(e);
						break;

					//global quit, not only the window's exit button
					case SDL.SDL_EventType.SDL_QUIT:
						Exit();
						break;
				}
			}

			Input.UpdateMousePosition();
			Input.ApplyButtonMaps();

			//update the weakreferences if they still exist
			Resources.UpdateManagedSprites();

			foreach (GameObject obj in Resources.Objects)
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
			foreach (GameObject obj in Resources.Objects)
			{
				if (!obj.Destroyed)
					obj.Draw();
			}

			DrawX.DrawJobs.OrderBy((job) => job.Depth);

			SDL.SDL_SetRenderDrawColor(RendererHandle, DrawX.BackgroundColor.R, DrawX.BackgroundColor.G, DrawX.BackgroundColor.B, DrawX.BackgroundColor.A);
			SDL.SDL_RenderClear(RendererHandle);

			foreach (View view in Resources.Views.Values)
			{
				if (!view.Active)
					continue;

				Rectangle absoluteGameArea = view.GetAbsoluteGameArea();
				Rectangle absoluteViewport = view.GetAbsoluteViewport();

				Point scale = view.GetScale();

				SDL.SDL_RenderSetScale(RendererHandle, scale.X, scale.Y);

				//viewport is affected by scale for whatever reason, correct it
				Rectangle scaledViewport = new Rectangle(absoluteViewport);
				scaledViewport.Position /= scale;
				scaledViewport.Size /= scale;
				SDL.SDL_Rect sdlViewport = (SDL.SDL_Rect)scaledViewport;
				SDL.SDL_RenderSetViewport(RendererHandle, ref sdlViewport);

				//get all jobs that will draw inside this view
				foreach (IDrawJob job in DrawX.GetDrawJobsByArea(absoluteGameArea))
				{
					Type jobType = job.GetType();

					if (jobType == typeof(TextureDrawJob)) //draw a texture
					{
						TextureDrawJob textureDrawJob = (TextureDrawJob)job;
						SDL.SDL_Rect sourceRect = (SDL.SDL_Rect)textureDrawJob.SourceRect;
						SDL.SDL_Rect destRect = (SDL.SDL_Rect)new Rectangle(textureDrawJob.DestRect.Position - absoluteGameArea.Position - textureDrawJob.DestRect.Origin, textureDrawJob.DestRect.Size);

						if (textureDrawJob.DestRect.Rotation == 0f)
							SDL.SDL_RenderCopy(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect);
						else
						{
							SDL.SDL_Point origin = (SDL.SDL_Point)textureDrawJob.DestRect.Origin;
							SDL.SDL_RenderCopyEx(RendererHandle, textureDrawJob.Texture, ref sourceRect, ref destRect, textureDrawJob.DestRect.Rotation, ref origin, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
						}
					}
					else if (jobType == typeof(LineDrawJob)) //draw line(s)
					{
						LineDrawJob lineDrawJob = (LineDrawJob)job;

						//transform all points according to view and cast em
						SDL.SDL_Point[] sdlPoints = Array.ConvertAll<Point, SDL.SDL_Point>(lineDrawJob.Points, point => (SDL.SDL_Point)(point - absoluteGameArea.Position));

						SDL.SDL_SetRenderDrawColor(RendererHandle, lineDrawJob.Color.R, lineDrawJob.Color.G, lineDrawJob.Color.B, lineDrawJob.Color.A);
						SDL.SDL_RenderDrawLines(RendererHandle, sdlPoints, lineDrawJob.PointCount);
					}
					else //draw filledrect
					{
						FilledRectDrawJob rectDrawJob = (FilledRectDrawJob)job;

						SDL.SDL_Rect rect = (SDL.SDL_Rect)(rectDrawJob.Area - absoluteGameArea.Position);

						SDL.SDL_SetRenderDrawColor(RendererHandle, rectDrawJob.Color.R, rectDrawJob.Color.G, rectDrawJob.Color.B, rectDrawJob.Color.A);
						SDL.SDL_RenderFillRect(RendererHandle, ref rect);
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
			SDL.SDL_RenderPresent(RendererHandle);
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

				SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Game Crash", message, WindowHandle);
			}
		}

		/// <summary>
		/// Converts a per-second value to a per-step value based on the game's speed, so you can use relative values.
		/// Basically all this does is take your value and divide it by the game's speed, but it's more logical and less bug prone if using this method.
		/// </summary>
		public static float ValuePerStep(float valuePerSecond)
		{
			return valuePerSecond / StepsPerSecond;
		}
	}
}