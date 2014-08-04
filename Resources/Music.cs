using System;
using SDL2_mixer;
using System.IO;
using System.Reflection;

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

			Mix.VolumeMusic((int)(128 * BaseVolume * volume));

			if (looping)
				Mix.PlayMusic(MusicHandle, -1);
			else
				Mix.PlayMusic(MusicHandle, 0);

			Resources.CurrentlyPlayingMusic = this;
		}

		public void Pause()
		{
			if (IsPlaying())
			{
				Mix.PauseMusic();
				Paused = true;
			}
		}

		public void Resume()
		{
			if (Paused && IsPlaying())
			{
				Mix.ResumeMusic();
				Paused = false;
			}
		}

		public void Stop()
		{
			if (IsPlaying())
				Mix.HaltMusic();
		}

		public bool IsPlaying()
		{
			return (Resources.CurrentlyPlayingMusic == this);
		}

		public void Load()
		{
			if (Loaded)
				return;

			MusicHandle = Mix.LoadMUS(Filename);

			if (MusicHandle != IntPtr.Zero)
				Loaded = true;
			else
				throw new FileLoadException();
		}

		public void Unload()
		{
			if (!Loaded)
				return;

			Mix.FreeMusic(MusicHandle);
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
	}
}