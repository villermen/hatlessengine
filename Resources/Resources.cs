using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		/// <summary>
		/// Contains all created resources by type and id.
		/// </summary>
		internal static Dictionary<Type, Dictionary<string, Resource>> Collection = new Dictionary<Type, Dictionary<string, Resource>>();

		/// <summary>
		/// Returns the resource with the specified type and id, or null if it doesn't exist.
		/// </summary>
		public static TResource Get<TResource>(string id) where TResource : Resource
		{
			//check for existence
			if (!Collection.ContainsKey(typeof(TResource)) || !Collection[typeof(TResource)].ContainsKey(id))
				throw new ResourceNotFoundException(String.Format("A resource of type {0} with id '{1}' does not exist.", typeof(TResource).Name, id));

			return Collection[typeof(TResource)][id] as TResource;
		}

		/// <summary>
		/// Returns all resources of the specified type.
		/// </summary>
		public static List<TResource> Get<TResource>() where TResource : Resource
		{
			//return empty list if type does not exist
			if (!Collection.ContainsKey(typeof(TResource)))
				return new List<TResource>();

			return Collection[typeof(TResource)].Values.Cast<TResource>().ToList();
		}

		internal static List<ExternalResource> ExternalResources = new List<ExternalResource>();

		public static List<GameObject> Objects = new List<GameObject>();
		public static List<PhysicalObject> PhysicalObjects = new List<PhysicalObject>();
		public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

		//addition/removal (has to be done after looping)
		internal static List<GameObject> AddObjects = new List<GameObject>();
		internal static List<GameObject> RemoveObjects = new List<GameObject>();

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
				return new BinaryReader(File.Open(RootDirectory + filename,
					FileMode.Open, FileAccess.Read, FileShare.Read));
			}

			if (File.Exists(filename))
			{
				return new BinaryReader(File.Open(filename,
					FileMode.Open, FileAccess.Read, FileShare.Read));
			}

			throw new FileNotFoundException("The file could not be found in any of the possible locations.");
		}

		/// <summary>
		/// Creates an SDL.SDL_RW resource from the entire file, using GetStream to resolve the filename.
		/// </summary>
		internal static IntPtr CreateRWFromFile(string filename, Assembly assembly)
		{
			using (BinaryReader reader = GetStream(filename, assembly))
			{
				int length = (int)reader.BaseStream.Length;
				return SDL.SDL_RWFromMem(reader.ReadBytes(length), length);
			}
		}

		public static void LoadAllExternalResources()
		{
			foreach (ExternalResource resource in ExternalResources)
				resource.Load();
		}
		public static void UnloadAllExternalResources()
		{
			foreach (ExternalResource resource in ExternalResources)
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
			if (CurrentlyPlayingMusic == null) 
				return;

			//CurrentlyPlayMusic.PerformStopped might change CurrentlyPlayingMusic itself, so use a temp value
			Music tempCurrentlyPlayingMusic = CurrentlyPlayingMusic;
			CurrentlyPlayingMusic = null;
			tempCurrentlyPlayingMusic.PerformStopped();
		}

		internal static void CleanupFontTextures()
		{
			foreach (Font font in Get<Font>())
			{
				List<Tuple<string, Color>> removeTextures = new List<Tuple<string, Color>>();

				foreach(KeyValuePair<Tuple<string, Color>, IntPtr> texture in font.Textures)
				{
					//delete texture if it hasn't been used for 10 steps
					if (font.TexturesDrawsUnused[texture.Key] == 10)
					{
						SDL.SDL_DestroyTexture(texture.Value);
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