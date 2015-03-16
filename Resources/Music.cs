using System;
using System.IO;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a music file that can be stream-played.
	/// </summary>
	public sealed class Music : ExternalResource
	{
		public float BaseVolume { get; private set; }
		public bool Paused { get; private set; }

		private IntPtr _musicHandle;

		public Music(string id, string file, float baseVolume = 1f)
			: base(id, file)
		{
			Loaded = false;
			BaseVolume = baseVolume;
			Paused = false;
		}

		/// <summary>
		/// Start playing the music.
		/// volume and balance will set the Volume and Balance before the music starts. (For convenience.)
		/// </summary>
		public void Play(bool looping = false, float volume = 1f, float balance = 0f)
		{
			if (!Loaded)
				throw new NotLoadedException();

			Paused = false;

			SDL_mixer.Mix_VolumeMusic((int)(128 * BaseVolume * volume));

			if (looping)
				SDL_mixer.Mix_PlayMusic(_musicHandle, -1);
			else
				SDL_mixer.Mix_PlayMusic(_musicHandle, 0);

			Resources.CurrentlyPlayingMusic = this;
		}

		public void Pause()
		{
			if (!IsPlaying()) 
				return;

			SDL_mixer.Mix_PauseMusic();
			Paused = true;
		}

		public void Resume()
		{
			if (!Paused || !IsPlaying()) 
				return;

			SDL_mixer.Mix_ResumeMusic();
			Paused = false;
		}

		public void Stop()
		{
			if (IsPlaying())
				SDL_mixer.Mix_HaltMusic();
		}

		public bool IsPlaying()
		{
			return Resources.CurrentlyPlayingMusic == this;
		}

		public override void Load()
		{
			if (Loaded)
				return;

			_musicHandle = SDL_mixer.Mix_LoadMUS(File);

			if (_musicHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public override void Unload()
		{
			if (!Loaded)
				return;

			SDL_mixer.Mix_FreeMusic(_musicHandle);
			Loaded = false;
		}

		public event EventHandler Stopped;

		internal void PerformStopped()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}

		public static implicit operator Music(string id)
		{
			return Resources.Get<Music>(id);
		}
	}
}