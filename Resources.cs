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

        public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        public static Dictionary<string, View> Views = new Dictionary<string, View>();
        public static Dictionary<string, Window> Windows = new Dictionary<string, Window>();
        public static Dictionary<string, Music> Musics = new Dictionary<string, Music>();
        public static Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();

        public static Sprite AddSprite(string id, string filename, float width = 0, float height = 0)
        {
            if (!File.Exists(RootDirectory + filename))
                Log.WriteLine("Resources.AddSprite: file '" + RootDirectory + filename + "' does not exist.", ErrorLevel.FATAL);
            if (Sprites.ContainsKey(id))
                Log.WriteLine("Resources.AddSprite: id '" + id + "' already exists.", ErrorLevel.FATAL);

            Sprite sprite;
            if (height == 0)
                sprite = new Sprite(RootDirectory + filename);
            else
                sprite = new Sprite(RootDirectory + filename, width, height);

            Sprites.Add(id, sprite);

            return sprite;
        }
        public static View AddView(string id, float viewX, float viewY, float viewWidth, float viewHeight, string targetWindow = "", float windowXFraction = 0, float windowYFraction = 0, float windowWidthFraction = 1, float windowHeightFraction = 1)
        {
            if (Views.ContainsKey(id))
                Log.WriteLine("Resources.AddView: id '" + id + "' already exists.", ErrorLevel.FATAL);
            View view = new View(id, viewX, viewY, viewWidth, viewHeight, targetWindow, windowXFraction, windowYFraction, windowWidthFraction);
            Views.Add(id, view);

            return view;
        }
        public static Window AddWindow(string id, uint width, uint height, string title)
        {
            if (Windows.ContainsKey(id))
                Log.WriteLine("Resources.AddWindow: id '" + id + "' already exists.", ErrorLevel.FATAL);

            Window window = new Window(id, width, height, title);
            Windows.Add(id, window);

            return window;
        }
        public static Music AddMusic(string id, string filename)
        {
            if (!File.Exists(RootDirectory + filename))
                Log.WriteLine("Resources.AddMusic: file '" + RootDirectory + filename + "' does not exist.", ErrorLevel.FATAL);
            if (Musics.ContainsKey(id))
                Log.WriteLine("Resources.AddMusic: id '" + id + "' already exists.", ErrorLevel.FATAL);

            Music music = new Music(RootDirectory + filename);
            Musics.Add(id, music);

            return music;
        }
        public static Sound AddSound(string id, string filename)
        {
            if (!File.Exists(RootDirectory + filename))
                Log.WriteLine("Resources.AddSound: file '" + RootDirectory + filename + "' does not exist.", ErrorLevel.FATAL);
            if (Sounds.ContainsKey(id))
                Log.WriteLine("Resources.AddSound: id '" + id + "' already exists.", ErrorLevel.FATAL);

            Sound sound = new Sound(RootDirectory + filename);
            Sounds.Add(id, sound);

            return sound;
        }

        public static Sprite Sprite(string id)
        {
            if (!Sprites.ContainsKey(id))
                Log.WriteLine("Resources.Sprite: id '" + id + "' does not exist.", ErrorLevel.FATAL);

            return Sprites[id];
        }
        public static View View(string id)
        {
            if (!Views.ContainsKey(id))
                Log.WriteLine("Resources.View: id '" + id + "' does not exist.", ErrorLevel.FATAL);

            return Views[id];
        }
        public static Window Window(string id)
        {
            if (!Windows.ContainsKey(id))
                Log.WriteLine("Resources.Window: id '" + id + "' does not exist.", ErrorLevel.FATAL);

            return Windows[id];
        }
        public static Music Music(string id)
        {
            if (!Musics.ContainsKey(id))
                Log.WriteLine("Resources.Sprite: id '" + id + "' does not exist.", ErrorLevel.FATAL);

            return Musics[id];
        }
        public static Sound Sound(string id)
        {
            if (!Sounds.ContainsKey(id))
                Log.WriteLine("Resources.Sprite: id '" + id + "' does not exist.", ErrorLevel.FATAL);

            return Sounds[id];
        }
    }
}
