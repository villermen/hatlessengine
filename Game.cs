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

		internal static bool RenderframeReady;

		private static bool _running;

		private static int _ticksPerStep;
		private static int _ticksPerDraw;

		/// <summary>
		/// If true the game will force to run steps at max frequency until it has caught up with the required speed.
		/// If not it will just keep going at regular speed after it goes out of sync.
		/// </summary>
		public static bool CatchUpSteps;

		/// <summary>
		/// Gets or sets the desired amount of steps per second.
		/// </summary>
		public static float StepsPerSecond
		{
			get { return Stopwatch.Frequency / _ticksPerStep; }
			set 
			{
				if (value > 0)
					_ticksPerStep = (int)(Stopwatch.Frequency / value);
				else
					throw new ArgumentOutOfRangeException("StepsPerSecond", "StepsPerSecond must be positive and nonzero. (" + value + " given)");
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
				if (_ticksPerDraw == 0)
					return 0;
				return Stopwatch.Frequency / _ticksPerDraw; 
			}
			set
			{
				if (value > 0)
					_ticksPerDraw = (int)(Stopwatch.Frequency / value);
				else if (value == 0f)
					_ticksPerDraw = 0;
				else
					throw new ArgumentOutOfRangeException("FPSLimit", "FpsLimit must be positive or zero. (" + value + " given)");
			}
		}

		static Game()
		{
			AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
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
			SDL_mixer.Mix_ChannelFinished(Resources.SoundChannelFinished);
			SDL_mixer.Mix_HookMusicFinished(Resources.MusicFinished);

			//initialize loop
			StepsPerSecond = speed;

			_running = true;			

			if (Started != null)
				Started(null, EventArgs.Empty);

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			long lastStepTick = 0;
			long lastDrawTick = 0;

			while (_running)
			{
				//perform step when needed
				if (stopWatch.ElapsedTicks >= lastStepTick + _ticksPerStep)
				{
					Profiler.Start("step");

					if (CatchUpSteps)
						lastStepTick = lastStepTick + _ticksPerStep;
					else
						lastStepTick = stopWatch.ElapsedTicks;

					Step();

					Profiler.Stop();
				}

				//perform draw when ready for a new one
				if (!RenderframeReady && _running && stopWatch.ElapsedTicks >= lastDrawTick + _ticksPerDraw)
				{
					Profiler.Start("draw");

					lastDrawTick = lastDrawTick + _ticksPerDraw;

					Draw();

					Profiler.Stop();
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
			Profiler.Start("collision");

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

			Profiler.Stop();

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

			DrawX.DrawJobs.OrderBy(job => job.Depth);

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
						SDL.SDL_Point[] sdlPoints = Array.ConvertAll(lineDrawJob.Points, point => (SDL.SDL_Point)(point - absoluteGameArea.Position));

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
			ThreadPool.QueueUserWorkItem(PresentRender);
		}

		/// <summary>
		/// Game will finish it's current loop (Step or Draw) and exit the Running method.
		/// </summary>
		public static void Exit()
		{
			_running = false;
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