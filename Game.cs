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

        static Game()
        {
            IsRunning = false;
            Thread.CurrentThread.Name = "HatlessEngine";
        }

        /// <summary>
        /// Run and create default logical object...
        /// If console is not enabled
        /// </summary>
        /// <param name="speed">Logical steps per second.</param>
        /// <param name="defaultWindowSetup">Creates a "default" view and window of 800x600</param>
        public static void Run(uint speed = 100, bool defaultWindowSetup = true)
        {
            Speed = speed;

            if (defaultWindowSetup)
            {
                Window window = Resources.AddWindow("default", 800, 600, "HatlessEngine");
                Resources.AddView("default", 0, 0, 800, 600, window, 0, 0, 1, 1);
            }

            IsRunning = true;

            //gameloop
            stopwatch.Start();
            while (stopwatch.IsRunning)
            {
                //progress towards next step from 0-1 used in twining operations (must be before step increment possibility)
                float stepProgress = Math.Max(Math.Min((stopwatch.ElapsedTicks - (Stepnumber * Stopwatch.Frequency / (float)Speed)) / (Stopwatch.Frequency / (float)Speed),1),0);

                //step
                if (stopwatch.ElapsedTicks >= (Stepnumber + 1) * Stopwatch.Frequency / (float)Speed)
                {
                    //handle window events before input (updates mouse position on windows and such)
                    Resources.WindowEvents();

                    //update input state
                    Input.UpdateState();

                    Resources.Step();

                    //window cleanup (cant be done during window-eventloop)
                    if (Resources.Windows.Count == 0 && Settings.ExitOnLastWindowClose)
                        Exit();
                    
                    //LPS calculation
                    TicksSinceLastStep = stopwatch.ElapsedTicks - LastStepTime;
                    SPS = (uint)(Stopwatch.Frequency / TicksSinceLastStep);
                    LastStepTime = stopwatch.ElapsedTicks;

                    Stepnumber++;
                }

                //draw
                Resources.Draw(stepProgress);
                
                //FPS calculation
                TicksSinceLastDraw = stopwatch.ElapsedTicks - LastDrawTime;
                FPS = (uint)(Stopwatch.Frequency / TicksSinceLastDraw);
                LastDrawTime = stopwatch.ElapsedTicks;
            }
        }

        public static void Exit()
        {
            //close open logfile
            Log.DisableFile();
            Environment.Exit(0);
        }
    }
}
