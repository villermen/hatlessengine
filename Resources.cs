using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Audio.OpenAL;
using System.Threading;

namespace HatlessEngine
{
    /// <summary>
    /// Will contain all references to the resource files.
    /// Keeps resources loaded until they are no longer needed, or aren't used for a while.
    /// </summary>
    public static class Resources
    {
		//private static string RootDirectory = System.Environment.CurrentDirectory + "/res/";

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
		public static Dictionary<Type, List<PhysicalObject>> PhysicalObjectsByType = new Dictionary<Type, List<PhysicalObject>>();

		internal static List<int> AudioSources = new List<int>();
		internal static Dictionary<int, AudioControl> AudioControls = new Dictionary<int, AudioControl>();

        //addition/removal (has to be done after looping)
		internal static List<LogicalObject> AddObjects = new List<LogicalObject>();
		internal static List<LogicalObject> RemoveObjects = new List<LogicalObject>();

		internal static bool MusicStreamerActive = false;
		internal static Thread MusicStreamerThread;

		internal static List<WeakReference> ManagedSprites = new List<WeakReference>();
		
		public static View AddView(RectangleF area, RectangleF viewport)
        {
			View view = new View(area, viewport);
            Views.Add(view);
            return view;
        }
		public static Sprite AddSprite(string id, string filename, Size size)
        {
            Sprite sprite;
			if (size == Size.Empty)
				sprite = new Sprite(id, filename);
            else
                sprite = new Sprite(id, filename, size);

            Sprites.Add(id, sprite);

            return sprite;
        }
        public static Sprite AddSprite(string id, string filename)
        {
			return AddSprite(id, filename, Size.Empty);
		}
        public static Font AddFont(string id, string filename)
        {
			Font font = new Font(id, filename);
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
        public static Objectmap AddObjectmap(string id, params ObjectmapBlueprint[] objectmapBlueprints)
        {
            Objectmap objectmap = new Objectmap(id, objectmapBlueprints);
            Objectmaps.Add(id, objectmap);
            return objectmap;
        }
        public static Spritemap AddSpritemap(string id, params SpritemapBlueprint[] spritemapBlueprints)
        {
            Spritemap spritemap = new Spritemap(id, spritemapBlueprints);
            Spritemaps.Add(id, spritemap);
            return spritemap;
        }
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

		internal static void LaunchMusicStreamerThread()
		{
			if (!Resources.MusicStreamerActive)
			{
				MusicStreamerThread = new Thread(new ThreadStart(Resources.MusicStreamer));
				MusicStreamerThread.Name = "HatlessEngine MusicStreamer";
				MusicStreamerThread.IsBackground = true;
				MusicStreamerThread.Start();
			}
		}

		internal static void MusicStreamer()
		{
			MusicStreamerActive = true;

			//decides whether the thread should stay alive
			bool workNeedsDoing = true;

			while (workNeedsDoing)
			{
				workNeedsDoing = false;

				//work when there's workin' to do
				foreach(KeyValuePair<string, Music> pair in Music)
				{
					Music music = pair.Value;

					if (music.Streaming)
					{
						workNeedsDoing = true;

						int buffersProcessed;
						AL.GetSource(music.SourceId, ALGetSourcei.BuffersProcessed, out buffersProcessed);

						if (music.JustStartedPlaying)
						{
							//will force filling the 2 still empty buffers and make sure the activebuffer is right afterwards
							buffersProcessed = 2;
							music.ActiveBufferId = 1;
							music.JustStartedPlaying = false;
						}

						//if the music's done with a buffer, fill it again and append it
						while (buffersProcessed > 0)
						{
							AL.SourceUnqueueBuffer(music.SourceId);

							//fill the just released buffer
							int requestedSamples = music.WaveReader.SampleRate / 2 * music.WaveReader.Channels;
							int readSamples;
							short[] waveData = music.WaveReader.ReadSamples(requestedSamples, out readSamples);
							AL.BufferData(music.BufferIds[music.ActiveBufferId], music.WaveReader.ALFormat, waveData, readSamples * 2, music.WaveReader.SampleRate);
					
							AL.SourceQueueBuffer(music.SourceId, music.BufferIds[music.ActiveBufferId]);

							if (++music.ActiveBufferId == 3)
								music.ActiveBufferId = 0;

							//this was the last buffer, stop loading will ya
							if (readSamples != requestedSamples)
							{
								if (music.Loop)
								{
									music.WaveReader.Rewind(music.LoopStartSample);
								}
								else
								{
									music.Streaming = false; //reached end

									//tight looping by hijacking sound source for new music
									if (music.PlayAfterMusic != "")
									{
										Music newMusic = Resources.Music[music.PlayAfterMusic];
										newMusic.SourceId = music.SourceId;
										newMusic.WaveReader.Rewind();
										newMusic.Streaming = true;
									}
								}
							}

							buffersProcessed--;
						}
					}
				}
				Thread.Sleep(200); //cya in a fifth of a second!
			}
			MusicStreamerActive = false;
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
    }
}
