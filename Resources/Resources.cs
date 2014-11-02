using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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

		//external
		internal static List<IExternalResource> ExternalResources = new List<IExternalResource>();
		public static Dictionary<string, Cursor> Cursors = new Dictionary<string, Cursor>();
		public static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
		public static Dictionary<string, Music> Music = new Dictionary<string, Music>();
		public static Dictionary<string, Sound> Sounds = new Dictionary<string, Sound>();
		public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

		//logical
		public static Dictionary<string, View> Views = new Dictionary<string, View>();

		//collections
		public static Dictionary<string, Objectmap> Objectmaps = new Dictionary<string, Objectmap>();
		public static Dictionary<string, Spritemap> Spritemaps = new Dictionary<string, Spritemap>();

		//objects
		public static List<GameObject> Objects = new List<GameObject>();
		public static List<PhysicalObject> PhysicalObjects = new List<PhysicalObject>();
		public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

		//addition/removal (has to be done after looping)
		internal static List<GameObject> AddObjects = new List<GameObject>();
		internal static List<GameObject> RemoveObjects = new List<GameObject>();

		internal static List<WeakReference> ManagedSprites = new List<WeakReference>();

		//audio helpers
		internal static Dictionary<int, SoundControl> SoundControls = new Dictionary<int, SoundControl>();
		internal static Music CurrentlyPlayingMusic;

		/// <summary>
		/// Gets the BinaryReader of a file with the given filename.
		/// All resources are loaded this way.
		/// Priority: 
		/// 1: Embedded Resource in the RootDirectory within the calling assembly.
		/// 2: Embedded Resource in the calling assembly.
		/// 3: File in the RootDirectory.
		/// 4: File in the application's directory, or an absolute filepath.
		/// Also, don't work with backslashes, they are nasty and unaccounted for.
		/// </summary>
		public static BinaryReader GetStream(string filename)
		{
			return GetStream(filename, Assembly.GetCallingAssembly());
		}
		internal static BinaryReader GetStream(string filename, Assembly assembly)
		{
			Stream stream;
			
			if (RootDirectory != "")
			{
				stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + (RootDirectory + filename).Replace('/', '.'));
				if (stream != null)
					return new BinaryReader(stream);
			}

			stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + filename.Replace('/', '.'));
			if (stream != null)
				return new BinaryReader(stream);

			if (RootDirectory != "" && File.Exists(RootDirectory + filename))
			{
				stream = File.Open(RootDirectory + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (stream != null)
					return new BinaryReader(stream);
			}

			if (File.Exists(filename))
			{
				stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (stream != null)
					return new BinaryReader(stream);
			}

			throw new FileNotFoundException("The file could not be found in any of the possible locations.");
		}

		/// <summary>
		/// Creates an SDL RW resource from the entire file, using GetStream to resolve the filename.
		/// </summary>
		internal static IntPtr CreateRWFromFile(string filename, Assembly assembly)
		{
			using (BinaryReader reader = GetStream(filename, assembly))
			{
				int length = (int)reader.BaseStream.Length;
				return SDL.RWFromMem(reader.ReadBytes(length), length);
			}
		}

		public static void LoadAllExternalResources()
		{
			foreach (IExternalResource resource in ExternalResources)
				resource.Load();
		}
		public static void UnloadAllExternalResources()
		{
			foreach (IExternalResource resource in ExternalResources)
				resource.Unload();
		}

		/// <summary>
		/// Destroys and removes all objects that are going to be added after this step.
		/// </summary>
		public static void CancelObjectCreation()
		{
			AddObjects.ForEach(obj => obj.Destroy());
			AddObjects.Clear();
		}

		internal static void ObjectAdditionAndRemoval()
		{
			//object addition
			Objects.AddRange(AddObjects);
			AddObjects.Clear();

			//object removal
			foreach (GameObject logicalObject in RemoveObjects)
				Objects.Remove(logicalObject);
			RemoveObjects.Clear();
		}

		/// <summary>
		/// Destroys a SoundControl when it's no longer supposed to exist.
		/// </summary>
		internal static void SoundChannelFinished(int channel)
		{
			SoundControls[channel].Destroy();
		}

		internal static void MusicFinished()
		{
			if (CurrentlyPlayingMusic != null)
			{
				//CurrentlyPlayMusic.PerformStopped might change CurrentlyPlayingMusic itself, so use a temp value
				Music tempCurrentlyPlayingMusic = CurrentlyPlayingMusic;
				CurrentlyPlayingMusic = null;
				tempCurrentlyPlayingMusic.PerformStopped();
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
			foreach (Font font in Fonts.Values)
			{
				List<Tuple<string, Color>> removeTextures = new List<Tuple<string, Color>>();

				foreach(KeyValuePair<Tuple<string, Color>, IntPtr> texture in font.Textures)
				{
					//delete texture if it hasn't been used for 10 steps
					if (font.TexturesDrawsUnused[texture.Key] == 10)
					{
						SDL.DestroyTexture(texture.Value);
						removeTextures.Add(texture.Key);
					}

					font.TexturesDrawsUnused[texture.Key]++;
				}

				foreach (Tuple<string, Color> texture in removeTextures)
				{
					font.Textures.Remove(texture);
					font.TexturesDrawsUnused.Remove(texture);
				}
			}
		}
	}
}