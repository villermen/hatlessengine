using System;
using System.IO;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Will contain all references to the resource files.
    /// Keeps resources loaded until they are no longer needed, or aren't used for a while.
    /// </summary>
    public static class Resources
    {
        private static string RootDirectory = System.Environment.CurrentDirectory + "/res/";

        //resources
        public static Dictionary<string, Window> Windows = new Dictionary<string, Window>();
        public static Dictionary<string, View> Views = new Dictionary<string, View>();
        public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        public static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
        public static Dictionary<string, Music> Music = new Dictionary<string, Music>();
        public static Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();

        //collections
        public static Dictionary<string, Objectmap> Objectmaps = new Dictionary<string, Objectmap>();
        public static Dictionary<string, Spritemap> Spritemaps = new Dictionary<string, Spritemap>();
        //public static Dictionary<string, CombinedMap> CombinedMaps = new Dictionary<string, CombinedMap>();

        //objects
        public static List<LogicalObject> Objects = new List<LogicalObject>();
        public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

        //addition/removal (has to be done after looping)
        internal static List<Window> RemoveWindows = new List<Window>();
        internal static List<LogicalObject> AddObjects = new List<LogicalObject>();
        internal static List<LogicalObject> RemoveObjects = new List<LogicalObject>();

        //window
        internal static SFML.Graphics.RenderTexture RenderPlane = new SFML.Graphics.RenderTexture(800, 600);
        private static SFML.Graphics.Sprite RenderSprite;
        private static SFML.Graphics.RectangleShape dirtyRenderFix = new SFML.Graphics.RectangleShape();
        public static Window FocusedWindow { get; internal set; }

        static Resources()
        {
            RenderPlane.Smooth = true;
            FocusedWindow = null;

            //add console font (implement stream loading)
            //Fonts.Add ("inconsolata", new Font("inconsolata", System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("HatlessEngine.Inconsolata.otf")));
        }

        public static Window AddWindow(string id, uint width, uint height, string title)
        {
            Window window = new Window(id, width, height, title);
            Windows.Add(id, window);
            return window;
        }
        public static View AddView(string id, float viewX, float viewY, float viewWidth, float viewHeight, Window targetWindow, float windowXFraction = 0, float windowYFraction = 0, float windowWidthFraction = 1, float windowHeightFraction = 1)
        {
            View view = new View(id, viewX, viewY, viewWidth, viewHeight, targetWindow, windowXFraction, windowYFraction, windowWidthFraction);
            Views.Add(id, view);
            return view;
        }
        public static Sprite AddSprite(string id, string filename, Size size)
        {
            /* old system
             * if (!File.Exists(RootDirectory + filename))
             *    Log.Message("Resources.AddSprite: file '" + RootDirectory + filename + "' does not exist.", ErrorLevel.FATAL);
             * if (Sprites.ContainsKey(id))
             *   Log.Message("Resources.AddSprite: id '" + id + "' already exists.", ErrorLevel.FATAL);
             */

            Sprite sprite;
            if (size.Width == 0 && size.Height == 0)
                sprite = new Sprite(id, RootDirectory + filename);
            else
                sprite = new Sprite(id, RootDirectory + filename, size);

            Sprites.Add(id, sprite);

            return sprite;
        }
        public static Sprite AddSprite(string id, string filename)
        {
            return AddSprite(id, filename, new Size(0, 0));
        }
        public static Font AddFont(string id, string filename)
        {
            Font font = new Font(id, RootDirectory + filename);
            Fonts.Add(id, font);
            return font;
        }
        public static Music AddMusic(string id, string filename)
        {
            Music music = new Music(id, RootDirectory + filename);
            Music.Add(id, music);
            return music;
        }
        public static Sound AddSound(string id, string filename)
        {
            Sound sound = new Sound(id, RootDirectory + filename);
            Sounds.Add(id, sound);
            return sound;
        }
        public static Objectmap AddObjectmap(string id, params ObjectmapBlueprint[] objects)
        {
            Objectmap objectMap = new Objectmap(id, objects);
            Objectmaps.Add(id, objectMap);
            return objectMap;
        }

        /// <summary>
        /// Performs step event for all resources that need it.
        /// Also handles addition and removal of resources after doing the previous.
        /// </summary>
        internal static void Step()
        {
            //step
            foreach (LogicalObject obj in Objects)
            {
                obj.Step();
                obj.AfterStep();
            }

            //object addition
            Objects.AddRange(AddObjects);
            AddObjects.Clear();

            //object removal
            foreach (LogicalObject logicalObject in RemoveObjects)
                Objects.Remove(logicalObject);
            RemoveObjects.Clear();

            //window removal
            foreach (Window window in RemoveWindows)
                Windows.Remove(window.Id);
            RemoveWindows.Clear();
        }

        /// <summary>
        /// Draws frame for every resource (so pretty much all drawing).
        /// </summary>
        public static void Draw(float stepProgress)
        {
            if (Resources.Windows.Count > 0)
            {
                //create texture to display
                RenderPlane.Clear(new SFML.Graphics.Color(64, 64, 64));

                //draw every objects' draw method
                foreach (LogicalObject obj in Objects)
                {
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
                foreach (KeyValuePair<string, Window> pair in Windows)
                {
                    Window window = pair.Value;
                    foreach (View view in window.ActiveViews)
                    {
                        window.SFMLWindow.SetView(view.SFMLView);
                        window.SFMLWindow.Draw(RenderSprite);
                    }
                    window.SFMLWindow.Display();
                }
            }
        }

        internal static void WindowEvents()
        {
            //window handling, before Input.UpdateState to have the correct mouse coordinates to work with
            foreach (KeyValuePair<string, Window> pair in Resources.Windows)
            {
                pair.Value.SFMLWindow.DispatchEvents();
            }
        }
    }
}
