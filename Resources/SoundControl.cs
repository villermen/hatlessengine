using System;
using SDL2;

namespace HatlessEngine
{
	/// <summary>
	/// Provides methods and events for controlling a specific instance of a sound playing.
	/// </summary>
	public class SoundControl
	{
		private readonly int _channel;
		public bool Done { get; private set; }

		internal SoundControl(int channel)
		{
			_channel = channel;
			Resources.SoundControls.Add(channel, this);
			Done = false;
		}

		public void Pause()
		{
			if (Done)
				return;

			SDL_mixer.Mix_Pause(_channel);
		}

		public void Resume()
		{
			if (Done)
				return;

			SDL_mixer.Mix_Resume(_channel);
		}

		public void Stop()
		{
			if (Done)
				return;

			SDL_mixer.Mix_HaltChannel(_channel);
		}

		public event EventHandler Stopped;

		internal void Destroy()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);

			Resources.SoundControls.Remove(_channel);
			Done = true;
		}
	}
}

