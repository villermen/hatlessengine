using System;
using System.IO;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// A sound effect that can be played multiple times simultaneously and is loaded into memory in it's entirity.
	/// Supports the following file formats taken from SDL.SDL_SDL_mixer.Mix_r: WAVE, AIFF, RIFF, OGG, and VOC.
	/// </summary>
	public class Sound : ExternalResource
	{
		public float BaseVolume { get; private set; }

		private IntPtr _chunkHandle;

		public Sound(string id, string file, float baseVolume = 1f)
			: base(id, file)
		{
			BaseVolume = baseVolume;
		}

		public SoundControl Play(float volume = 1f, float balance = 0f)
		{
			if (!Loaded)
				throw new NotLoadedException();

			int channel = SDL_mixer.Mix_PlayChannelTimed(-1, _chunkHandle, 0, -1);
			SDL_mixer.Mix_Volume(channel, (int)(128 * volume));
			balance = balance / 2f + 0.5f; //0-1
			SDL_mixer.Mix_SetPanning(channel, (byte)(255 - 255 * balance), (byte)(0 + 255 * balance));

			return new SoundControl(channel);
		}

		public override void Load()
		{
			if (Loaded)
				return;

			_chunkHandle = SDL_mixer.Mix_LoadWAV_RW(Resources.CreateRWFromFile(File, FileAssembly), 1);

			if (_chunkHandle != IntPtr.Zero)
			{
				SDL_mixer.Mix_VolumeChunk(_chunkHandle, (int)(128 * BaseVolume));
				Loaded = true;
			}
			else
				throw new FileLoadException();
		}

		public override void Unload()
		{
			if (!Loaded)
				return;

			SDL_mixer.Mix_FreeChunk(_chunkHandle);
			_chunkHandle = IntPtr.Zero;

			Loaded = false;
		}

		public static implicit operator Sound(string id)
		{
			return Resources.Get<Sound>(id);
		}
	}
}
