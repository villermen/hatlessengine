using System;
using SDL2_mixer;

namespace HatlessEngine
{
	/// <summary>
	/// Provides methods and events for controlling a specific instance of a sound playing.
	/// </summary>
	public class SoundControl
	{
		private int Channel;
		public bool Done { get; private set; }

		internal SoundControl(int channel)
		{
			Channel = channel;
			Resources.SoundControls.Add(channel, this);
			Done = false;
		}

		public void Pause()
		{
			if (Done)
				return;

			Mix.Pause(Channel);
		}

		public void Resume()
		{
			if (Done)
				return;

			Mix.Resume(Channel);
		}

		public void Stop()
		{
			if (Done)
				return;

			Mix.HaltChannel(Channel);
		}

		public event EventHandler Stopped;

		internal void Destroy()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);

			Resources.SoundControls.Remove(Channel);
			Done = true;
		}
	}
}

