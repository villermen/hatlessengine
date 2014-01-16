using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	/// <summary>
	/// Manipulates playing audio. Inherited by SoundControl and Music.
	/// </summary>
	public class AudioControl
	{
		internal int SourceId;

		internal AudioControl() { }

		protected float _Volume = 1f;

		public float Volume
		{
			get 
			{ 
				return _Volume;
			}
			set 
			{ 
				if (IsPlaying())
					AL.Source(SourceId, ALSourcef.Gain, value);
				_Volume = value;
			}
		}

		public void Pause()
		{
			AL.SourcePause(SourceId);
		}
		public void Resume()
		{
			AL.SourcePlay(SourceId);
		}
		public void Stop()
		{
			AL.SourceStop(SourceId);
		}

		public bool IsPlaying()
		{
			return (AL.GetSourceState(SourceId) == ALSourceState.Playing);
		}
		public bool IsPaused()
		{
			return (AL.GetSourceState(SourceId) == ALSourceState.Paused);
		}
		public bool IsStopped()
		{
			return (AL.GetSourceState(SourceId) == ALSourceState.Stopped);
		}

		public event EventHandler Stopped;
		internal void PerformStopped()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}
	}
}
