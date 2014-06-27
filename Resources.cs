using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Audio.OpenAL;
using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Will contain all references to the resource files.
	/// Keeps resources loaded until they are no longer needed, or aren't used for a while.
	/// </summary>
	public static class Resources
	{
		/// <summary>
		/// If set this will be checked before the program's location will be checked.
		/// It will do relative checks if no drive is supplied.
		/// It will also work for embedded resources.
		/// Make sure to add a trailing slash and leave out a leading one. (if relative)
		/// </summary>
		public static string RootDirectory = "";

		//resources
		public static List<View> Views = new List<View>();
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
		public static List<PhysicalObject> PhysicalObjects = new List<PhysicalObject>();
		public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

		internal static List<int> AudioSources = new List<int>();
		internal static Dictionary<int, AudioControl> AudioControls = new Dictionary<int, AudioControl>();

		//addition/removal (has to be done after looping)
		internal static List<LogicalObject> AddObjects = new List<LogicalObject>();
		internal static List<LogicalObject> RemoveObjects = new List<LogicalObject>();

		internal static List<WeakReference> ManagedSprites = new List<WeakReference>();

		/// <summary>
		/// Gets the StreamReader of a (resource) file with the given filename.
		/// All resources are loaded this way.
		/// Priority: 
		/// 1: Embedded Resource in the RootDirectory withing the entry assembly;
		/// 2: Embedded Resource in the entry assembly.
		/// 3: File in the RootDirectory.
		/// 4: File in the application's directory, or an absolute filepath.
		/// Also, don't work with backslashes, they are nasty and unaccounted for.
		/// </summary>
		public static BinaryReader GetStream(string fileName)
		{
			Stream stream;

			Assembly entryAssembly = Assembly.GetEntryAssembly();

			if (RootDirectory != "")
			{
				stream = entryAssembly.GetManifestResourceStream(entryAssembly.GetName().Name + "." + (RootDirectory + fileName).Replace('/', '.'));
				if (stream != null)
					return new BinaryReader(stream);
			}

			stream = entryAssembly.GetManifestResourceStream(entryAssembly.GetName().Name + "." + fileName.Replace('/', '.'));
			if (stream != null)
				return new BinaryReader(stream);

			if (RootDirectory != "" && File.Exists(RootDirectory + fileName))
			{
				stream = File.Open(RootDirectory + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);			
				if (stream != null)
					return new BinaryReader(stream);
			}

			if (File.Exists(fileName))
			{
				stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (stream != null)
					return new BinaryReader(stream);
			}

			throw new FileNotFoundException("The file could not be found in any of the possible locations.");
		}

		public static View AddView(SimpleRectangle area, SimpleRectangle viewport)
		{
			View view = new View(area, viewport);
			Views.Add(view);
			return view;
		}
		public static Sprite AddSprite(string id, string filename, Point size)
		{
			Sprite sprite;
			if (size == new Point(0f, 0f))
				sprite = new Sprite(id, filename);
			else
				sprite = new Sprite(id, filename, size);

			Sprites.Add(id, sprite);

			return sprite;
		}
		public static Sprite AddSprite(string id, string filename)
		{
			return AddSprite(id, filename, new Point(0f, 0f));
		}
		public static Font AddFont(string id, string filename, int pointSize)
		{
			Font font = new Font(id, filename, pointSize);
			Fonts.Add(id, font);
			return font;
		}
		public static Music AddMusic(string id, string filename)
		{
			Music music = new Music(id, filename);
			Music.Add(id, music);
			return music;
		}
		public static Sound AddSound(string id, string filename)
		{
			Sound sound = new Sound(id, filename);
			Sounds.Add(id, sound);
			return sound;
		}
		public static Objectmap AddObjectmap(string id, params ObjectBlueprint[] objectmapBlueprints)
		{
			Objectmap objectmap = new Objectmap(id, objectmapBlueprints);
			Objectmaps.Add(id, objectmap);
			return objectmap;
		}
		/// <summary>
		/// Add an Objectmap from file (saved by Objectmap.WriteToFile)
		/// </summary>
		public static Objectmap AddObjectmap(string id, string filename)
		{
			Objectmap objectmap = new Objectmap(id, filename);
			Objectmaps.Add(id, objectmap);
			return objectmap;
		}
		public static Spritemap AddSpritemap(string id, params ManagedSprite[] managedSprites)
		{
			Spritemap spritemap = new Spritemap(id, managedSprites);
			Spritemaps.Add(id, spritemap);
			return spritemap;
		}
		/// <summary>
		/// Add a Spritemap from file (saved by Spritemap.WriteToFile)
		/// </summary>
		public static Spritemap AddSpritemap(string id, string filename)
		{
			Spritemap spritemap = new Spritemap(id, filename);
			Spritemaps.Add(id, spritemap);
			return spritemap;
		}

		public static void LoadAllExternalResources()
		{
			foreach(KeyValuePair<string, Sprite> pair in Sprites)
				pair.Value.Load();
			foreach(KeyValuePair<string, Font> pair in Fonts)
				pair.Value.Load();
			foreach(KeyValuePair<string, Sound> pair in Sounds)
				pair.Value.Load();
			foreach(KeyValuePair<string, Music> pair in Music)
				pair.Value.Load();
		}
		public static void UnloadAllExternalResources()
		{
			foreach (KeyValuePair<string, Sprite> pair in Sprites)
				pair.Value.Unload();
			foreach (KeyValuePair<string, Font> pair in Fonts)
				pair.Value.Unload();
			foreach (KeyValuePair<string, Sound> pair in Sounds)
				pair.Value.Unload();
			foreach (KeyValuePair<string, Music> pair in Music)
				pair.Value.Unload();
		}

		internal static void ObjectAdditionAndRemoval()
		{
			//object addition
			Objects.AddRange(AddObjects);
			AddObjects.Clear();

			//object removal
			foreach (LogicalObject logicalObject in RemoveObjects)
				Objects.Remove(logicalObject);
			RemoveObjects.Clear();
		}

		/// <summary>
		/// Gets an OpenAL source identifier.
		/// Source will be managed by HatlessEngine to prevent not playing of sound after all device channels are occupied.
		/// </summary>
		internal static int GetSource()
		{
			int source;
			//will execute multiple times if cleanup has not removed the source from AudioControls yet
			while (AudioControls.ContainsKey(source = AL.GenSource())) { }
			AudioSources.Add(source);
			return source;
		}

		/// <summary>
		/// Removes all stopped sources.
		/// </summary>
		internal static void SourceRemoval()
		{
			List<int> removeSources = new List<int>();
			foreach(int source in AudioSources)
			{
				if (AL.GetSourceState(source) == ALSourceState.Stopped)
				{
					AL.DeleteSource(source);
					removeSources.Add(source);
				}
			}

			foreach(int source in removeSources)
			{
				AudioSources.Remove(source);
				AudioControls[source].PerformStopped();
				AudioControls.Remove(source);
			}
		}

		internal static void UpdateManagedSprites()
		{
			List<WeakReference> removeManagedSprites = new List<WeakReference>();
			foreach(WeakReference managedSprite in ManagedSprites)
			{
				//check if alive and add to remove list if not
				if (managedSprite.IsAlive)
				{
					//perform step
					((ManagedSprite)managedSprite.Target).Step();
				}
				else
					removeManagedSprites.Add(managedSprite);
			}

			foreach(WeakReference managedSprite in removeManagedSprites)
			{
				ManagedSprites.Remove(managedSprite);
			}
		}

		internal static void CleanupFontTextures()
		{
			foreach (KeyValuePair<string, Font> font in Fonts)
			{
				List<Tuple<string, Color>> removeTextures = new List<Tuple<string, Color>>();

				foreach(KeyValuePair<Tuple<string, Color>, IntPtr> texture in font.Value.Textures)
				{
					//delete texture if it hasn't been used for 3 draw steps
					if (font.Value.TexturesDrawsUnused[texture.Key] == 3)
					{
						SDL.SDL_DestroyTexture(texture.Value);
						removeTextures.Add(texture.Key);
					}

					font.Value.TexturesDrawsUnused[texture.Key]++;
				}

				foreach (Tuple<string, Color> texture in removeTextures)
				{
					font.Value.Textures.Remove(texture);
					font.Value.TexturesDrawsUnused.Remove(texture);
				}
			}
		}
	}
}