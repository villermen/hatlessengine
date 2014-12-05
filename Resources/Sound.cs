using System;
using System.IO;
using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// A sound effect that can be played multiple times simultaneously and is loaded into memory in it's entirity.
	/// Supports the following file formats taken from SDL.SDL_SDL_mixer.Mix_r: WAVE, AIFF, RIFF, OGG, and VOC.
	/// </summary>
	public class Sound : IExternalResource
	{
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public bool Loaded { get; private set; }

		public float BaseVolume { get; private set; }

		private IntPtr ChunkHandle;

		public Sound(string id, string filename, float baseVolume = 1f)
		{
			ID = id;
			Filename = filename;
			FileAssembly = Assembly.GetCallingAssembly();
			Loaded = false;

			BaseVolume = baseVolume;

			Resources.Sounds.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public SoundControl Play(float volume = 1f, float balance = 0f)
		{
			if (!Loaded)
				throw new NotLoadedException();

			int channel = SDL_mixer.Mix_PlayChannelTimed(-1, ChunkHandle, 0, -1);
			SDL_mixer.Mix_Volume(channel, (int)(128 * volume));
			balance = balance / 2f + 0.5f; //0-1
			SDL_mixer.Mix_SetPanning(channel, (byte)(255 - 255 * balance), (byte)(0 + 255 * balance));

			return new SoundControl(channel);
		}

		public void Load()
		{
			if (Loaded)
				return;

			ChunkHandle = SDL_mixer.Mix_LoadWAV_RW(Resources.CreateRWFromFile(Filename, FileAssembly), 1);

			if (ChunkHandle != IntPtr.Zero)
			{
				SDL_mixer.Mix_VolumeChunk(ChunkHandle, (int)(128 * BaseVolume));
				Loaded = true;
			}
			else
				throw new FileLoadException();
		}

		public void Unload()
		{
			if (!Loaded)
				return;

			SDL_mixer.Mix_FreeChunk(ChunkHandle);
			ChunkHandle = IntPtr.Zero;

			Loaded = false;
		}

		public void Destroy()
		{
			Unload();

			Resources.Sounds.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}

		public static implicit operator Sound(string str)
		{
			return Resources.Sounds[str];
		}

		public void Dispose()
		{
			Unload();
		}
	}
}