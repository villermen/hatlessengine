using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Audio;
using OpenTK.Graphics;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;

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
				obj.Step();

            //collision time!
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            foreach(PhysicalObject obj in Resources.PhysicalObjects)
            {
                obj.UpdateCoverableArea();

                if (obj.CoverableArea.X < minX)
                    minX = obj.CoverableArea.X;
                if (obj.CoverableArea.X2 > maxX)
                    maxX = obj.CoverableArea.X2;
                if (obj.CoverableArea.Y < minY)
                    minY = obj.CoverableArea.Y;
                if (obj.CoverableArea.Y2 > maxY)
                    maxY = obj.CoverableArea.Y2;

                //set before the actual collision check phase
                obj.SpeedLeft = 1f;
            }
            
            QuadTree quadTree = new QuadTree(new Rectangle(minX, minY, maxX - minX, maxY - minY));

            //maybe if too slow use SortedList and compare using the default comparer
            List<PhysicalObject> processingObjects = new List<PhysicalObject>(Resources.PhysicalObjects);       
         
            //get all first collision speedfractions for all objects and sort the list
            foreach (PhysicalObject obj in processingObjects)
                obj.CalculateClosestCollision(quadTree.GetPossibleTargets(obj));

            processingObjects.Sort(ComparePhysicalObjectsByFraction);

            while (processingObjects.Count > 0)            
            {
                //get closest collision, process it/the pair of objects
                PhysicalObject obj = processingObjects[0];
                obj.PerformClosestCollision();

                //remove/recalculate collisions for the object and all possibly influenced objects
                if (obj.SpeedLeft == 0f)
                    processingObjects.Remove(obj);
                else
                    obj.CalculateClosestCollision(quadTree.GetPossibleTargets(obj));
                
                foreach (PhysicalObject influencedObj in quadTree.GetPossibleTargets(obj))
                    influencedObj.CalculateClosestCollision(quadTree.GetPossibleTargets(obj));

                //re-sort so closest is up first again
                processingObjects.Sort(ComparePhysicalObjectsByFraction);
            }

			Resources.SourceRemoval();
			Resources.ObjectAdditionAndRemoval();

			//update input state (push buttonlist) for update before next step
			Input.PushButtons();
		}

        private static int ComparePhysicalObjectsByFraction(PhysicalObject obj1, PhysicalObject obj2)
        {
            if (obj1.ClosestCollisionSpeedFraction < obj2.ClosestCollisionSpeedFraction)
                return -1;
            else if (obj1.ClosestCollisionSpeedFraction > obj2.ClosestCollisionSpeedFraction)
                return 1;
            else
                return 0;
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

    /// <summary>
    /// Used by Game to quickly discover what objects one object could potentially interact with.
    /// </summary>
    internal class QuadTree
    {
        private static byte MaxObjects = 10;
        private static byte MaxLevels = 5;

        private int Level;
        private Rectangle Bounds;
        private float CenterX;
        private float CenterY;
        private QuadTree[] Children = new QuadTree[4];

        private List<PhysicalObject> Objects = new List<PhysicalObject>();

        /// <summary>
        /// The mother of all quadtrees.
        /// </summary>
        public QuadTree(Rectangle bounds)
            : this(0, bounds, Resources.PhysicalObjects) { }

        /// <summary>
        /// A teensy quadtree baby.
        /// </summary>
        private QuadTree(int level, Rectangle bounds, List<PhysicalObject> objects)
        {
            Level = level;
            Bounds = bounds;
            CenterX = Bounds.X + Bounds.Width / 2f;
            CenterY = Bounds.Y + Bounds.Height / 2f;

            if (objects.Count > MaxObjects)
            {
                //decide in what childtree an object would fit and add it there
                List<PhysicalObject> ChildObjects0 = new List<PhysicalObject>();
                List<PhysicalObject> ChildObjects1 = new List<PhysicalObject>();
                List<PhysicalObject> ChildObjects2 = new List<PhysicalObject>();
                List<PhysicalObject> ChildObjects3 = new List<PhysicalObject>();

                foreach (PhysicalObject obj in objects)
                {
                    bool[] fits = FitObject(obj);
                    if (!fits[4])
                    {
                        if (fits[0])
                            ChildObjects0.Add(obj);
                        else if (fits[1])
                            ChildObjects1.Add(obj);
                        else if (fits[2])
                            ChildObjects2.Add(obj);
                        else
                            ChildObjects3.Add(obj);
                    }
                    else
                        Objects.Add(obj);
                }

                //create subtrees and add everything that fits inside of em
                Children[0] = new QuadTree(Level + 1, new Rectangle(Bounds.X, Bounds.Y, CenterX, CenterY), ChildObjects0);
                Children[1] = new QuadTree(Level + 1, new Rectangle(CenterX, Bounds.Y, CenterX, CenterY), ChildObjects1);
                Children[2] = new QuadTree(Level + 1, new Rectangle(Bounds.X, CenterY, CenterX, CenterY), ChildObjects2);
                Children[3] = new QuadTree(Level + 1, new Rectangle(CenterX, CenterY, CenterX, CenterY), ChildObjects3);
            }
            else
                Objects = objects;
        }

        public List<PhysicalObject> GetPossibleTargets(PhysicalObject obj)
        {
            List<PhysicalObject> targets = new List<PhysicalObject>(Objects);

            //check in the child trees this object overlaps with
            if (Children[0] != null)
            {
                bool[] fits = FitObject(obj);
                if (fits[0])
                    targets.AddRange(Children[0].GetPossibleTargets(obj));
                if (fits[1])
                    targets.AddRange(Children[1].GetPossibleTargets(obj));
                if (fits[2])
                    targets.AddRange(Children[2].GetPossibleTargets(obj));
                if (fits[3])
                    targets.AddRange(Children[3].GetPossibleTargets(obj));
            }

            return targets;
        }

        /// <summary>
        /// Returns the childtrees the given object fits in.
        /// 0-3, left-to-right, top-to-bottom.
        /// 4 is true when the object doesn't fit in just one quadrant.
        /// </summary>
        private bool[] FitObject(PhysicalObject obj)
        {
            bool[] fits = new bool[5];
            byte quadrants = 0;

            if (obj.CoverableArea.X <= CenterX)
            {
                if (obj.CoverableArea.Y <= CenterY)
                {
                    fits[0] = true;
                    quadrants++;
                }
                if (obj.CoverableArea.Y2 >= CenterY)
                {
                    fits[2] = true;
                    quadrants++;
                }
            }
            if (obj.CoverableArea.X2 >= CenterX)
            {
                if (obj.CoverableArea.Y <= CenterY)
                {
                    fits[1] = true;
                    quadrants++;
                }
                if (obj.CoverableArea.Y2 >= CenterY)
                {
                    fits[3] = true;
                    quadrants++;
                }
            }

            if (quadrants > 1)
                fits[4] = true;

            return fits;
        }
    }
}

