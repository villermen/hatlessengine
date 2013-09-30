using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace HatlessEngine
{
    /// <summary>
    /// Provides gameloop, object/window handling
    /// </summary>
    public static class Game
    {
        public static uint Speed { get; private set; }
        private static long Stepnumber = 0;
        private static Stopwatch stopwatch = new Stopwatch();

        public static bool IsRunning { get; private set; }

        private static long TicksSinceLastStep = 0;
        private static long LastStepTime = 0;
        public static uint SPS = 0;
        private static long TicksSinceLastDraw = 0;
        private static long LastDrawTime = 0;
        public static uint FPS = 0;

        public static List<LogicalObject> Objects { get; private set; }
        public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType { get; private set; }

        internal static SFML.Graphics.RenderTexture RenderPlane = new SFML.Graphics.RenderTexture(800, 600);
        private static SFML.Graphics.Sprite RenderSprite;
        private static SFML.Graphics.RectangleShape dirtyRenderFix = new SFML.Graphics.RectangleShape();

        internal static List<Window> RemoveWindows = new List<Window>();

        public static Window FocusedWindow { get; internal set; }

        //debug
        private static string SPSDelayed = "";
        private static string FPSDelayed = "";

        static Game()
        {
            IsRunning = false;
            RenderPlane.Smooth = true;
            FocusedWindow = null;
            Objects = new List<LogicalObject>();
            PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

            Thread.CurrentThread.Name = "HatlessEngine";
        }

        /// <summary>
        /// Run and create default logical object...
        /// If console is not enabled
        /// </summary>
        /// <param name="speed">Logical steps per second.</param>
        /// <param name="defaultWindowSetup">Creates a "default" view and window of 800x600</param>
        public static void Run<T>(uint speed = 100, bool defaultWindowSetup = true) where T : LogicalObject
        {
            Speed = speed;

            if (defaultWindowSetup)
            {
                Window window = Resources.AddWindow("default", 800, 600, "HatlessEngine");
                Resources.AddView("default", 0, 0, 800, 600, window, 0, 0, 1, 1);
            }

            IsRunning = true;

            if (typeof(T) != typeof(LogicalObject))
                CreateLogicalObject<T>();

            //gameloop
            stopwatch.Start();
            while (stopwatch.IsRunning)
            {
                //progress towards next step from 0-1 used in twining operations (must be before step increment possibility)
                float stepProgress = Math.Max(Math.Min((stopwatch.ElapsedTicks - (Stepnumber * Stopwatch.Frequency / (float)Speed)) / (Stopwatch.Frequency / (float)Speed),1),0);

                //step
                if (stopwatch.ElapsedTicks >= (Stepnumber + 1) * Stopwatch.Frequency / (float)Speed)
                {
                    //window handling, before Input.UpdateState to have the correct mouse coordinates to work with
                    foreach (Window window in Resources.Windows)
                        window.SFMLWindow.DispatchEvents();

                    //update input state
                    Input.UpdateState();

                    //window cleanup (cant be done during window-eventloop)
                    foreach (Window window in RemoveWindows)
                        Resources.Windows.Remove(window);
                    RemoveWindows.Clear();

                    if (Resources.Windows.Count == 0 && Settings.ExitOnLastWindowClose)
                        Exit();

                    //objects
                    foreach (LogicalObject obj in Objects)
                    {
                        obj.BeforeStep();
                        obj.Step();
                        obj.AfterStep();
                    }
                    
                    //LPS calculation
                    TicksSinceLastStep = stopwatch.ElapsedTicks - LastStepTime;
                    SPS = (uint)(Stopwatch.Frequency / TicksSinceLastStep);
                    LastStepTime = stopwatch.ElapsedTicks;

                    /*string str = Input.GetPressedButtons(true);
                    Log.ClearConsole();
                    Log.Write("BUTTONTEST");
                    Log.Write(str);*/

                    //temp
                    if (Log.ConsoleEnabled)
                        Log.DispatchConsoleEvents();

                    Stepnumber++;
                }

                //draw
                if (Resources.Windows.Count > 0)
                {
                    //create texture to display
                    RenderPlane.Clear(new SFML.Graphics.Color(64, 64, 64));

                    //draw every objects' draw method
                    foreach (LogicalObject obj in Objects)
                    {
                        obj.BeforeDraw(stepProgress);
                        obj.Draw(stepProgress);
                        obj.AfterDraw(stepProgress);
                    }

                    //to prevent weird rendertexture bug
                    dirtyRenderFix.Size = new SFML.Window.Vector2f(RenderPlane.Size.X, RenderPlane.Size.Y);
                    dirtyRenderFix.FillColor = SFML.Graphics.Color.Transparent;
                    RenderPlane.Draw(dirtyRenderFix);

                    RenderPlane.Display();
                    RenderSprite = new SFML.Graphics.Sprite(RenderPlane.Texture);

                    //display the texture on window(s) using view(s)
                    foreach (Window window in Resources.Windows)
                    {
                        foreach (View view in window.ActiveViews)
                        {
                            window.SFMLWindow.SetView(view.SFMLView);
                            window.SFMLWindow.Draw(RenderSprite);
                        }

                        window.SFMLWindow.Display();
                    }

                    //FPS calculation
                    TicksSinceLastDraw = stopwatch.ElapsedTicks - LastDrawTime;
                    FPS = (uint)(Stopwatch.Frequency / TicksSinceLastDraw);
                    LastDrawTime = stopwatch.ElapsedTicks;
                }
            }
        }

        public static void Run(uint speed = 100, bool defaultWindowSetup = true)
        {
            Run<LogicalObject>(speed, defaultWindowSetup);
        }

        /// <summary>
        /// Creates Object with the given type.
        /// </summary>
        /// <param name="type">Type of the object to create (must be a child of GameObject)</param>
        /// <returns>The object's id</returns>
        public static LogicalObject CreateLogicalObject<T>() where T : LogicalObject
        {
            if (!IsRunning)
                Log.WriteLine("CreatePhysicalObject: Cannot create objects before game is started.", ErrorLevel.FATAL);

            Type type = typeof(T);

            LogicalObject obj = (LogicalObject)Activator.CreateInstance(type);
            Objects.Add(obj);

            obj.AfterCreation();

            return obj;
        }
        public static PhysicalObject CreatePhysicalObject<T>(float x, float y) where T : PhysicalObject
        {
            if (!IsRunning)
                Log.WriteLine("CreatePhysicalObject: Cannot create objects before game is started.", ErrorLevel.FATAL);

            Type type = typeof(T);

            PhysicalObject obj = (PhysicalObject)Activator.CreateInstance(type);
            Objects.Add(obj);

            obj.X = x;
            obj.Y = y;
            obj.AfterCreation();

            if (!PhysicalObjectsByType.ContainsKey(type))
                PhysicalObjectsByType[type] = new List<PhysicalObject>();
            PhysicalObjectsByType[type].Add(obj);

            return obj;
        }

        public static void Exit()
        {
            stopwatch.Stop();
        }
    }
}
