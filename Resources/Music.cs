using System;
using System.IO;
using System.Reflection;
using SDL2;

namespace HatlessEngine
{
	public sealed class Music : IExternalResource
	{
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public Assembly FileAssembly { get; private set; }
		public bool Loaded { get; private set; }

		public float BaseVolume { get; private set; }
		public bool Paused { get; private set; }

		private IntPtr MusicHandle;

		public Music(string id, string filename, float baseVolume = 1f)
		{
			ID = id;
			Filename = filename;
			Loaded = false;

			BaseVolume = baseVolume;
			Paused = false;

			Resources.Music.Add(ID, this);
			Resources.ExternalResources.Add(this);
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
				SDL_mixer.Mix_PlayMusic(MusicHandle, -1);
			else
				SDL_mixer.Mix_PlayMusic(MusicHandle, 0);

			Resources.CurrentlyPlayingMusic = this;
		}

		public void Pause()
		{
			if (IsPlaying())
			{
				SDL_mixer.Mix_PauseMusic();
				Paused = true;
			}
		}

		public void Resume()
		{
			if (Paused && IsPlaying())
			{
				SDL_mixer.Mix_ResumeMusic();
				Paused = false;
			}
		}

		public void Stop()
		{
			if (IsPlaying())
				SDL_mixer.Mix_HaltMusic();
		}

		public bool IsPlaying()
		{
			return (Resources.CurrentlyPlayingMusic == this);
		}

		public void Load()
		{
			if (Loaded)
				return;

			MusicHandle = SDL_mixer.Mix_LoadMUS(Filename);

			if (MusicHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public void Unload()
		{
			if (!Loaded)
				return;

			SDL_mixer.Mix_FreeMusic(MusicHandle);
			Loaded = false;
		}

		public event EventHandler Stopped;

		internal void PerformStopped()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}

		public void Destroy()
		{
			Unload();

			Resources.Music.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}

		public static implicit operator Music(string str)
		{
			return Resources.Music[str];
		}

		/// <summary>
		/// Pretty much an alias for Destroy(), here just to implement IDisposable as this object uses unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Destroy();

			//do not suppress finalization as the resource could be loaded after this point
		}

		~Music()
		{
			Dispose();
		}
	}
}