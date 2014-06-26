using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	/// <summary>
	/// Manipulates playing audio. Inherited by SoundControl and Music.
	/// </summary>
	public class AudioControl
	{
		internal int SourceID;

		internal AudioControl() { }

		protected float _Volume = 1f;
		/// <summary>
		/// [0f,1f]
		/// </summary>
		public float Volume
		{
			get 
			{ 
				return _Volume;
			}
			set 
			{ 
				AL.Source(SourceID, ALSourcef.Gain, value);
				_Volume = value;
			}
		}

		protected float _Balance = 0f;
		/// <summary>
		/// [-1f,1f], only works for mono audio.
		/// </summary>
		public float Balance
		{
			get
			{
				return _Balance;
			}
			set
			{
				AL.Source(SourceID, ALSource3f.Position, value, 0f, (float)Math.Sqrt(1 - Math.Pow(value, 2))); //Thanks to Ethan Lee from FNA
				_Balance = value;
			}
		}

		public void Pause()
		{
			AL.SourcePause(SourceID);
		}
		public void Resume()
		{
			AL.SourcePlay(SourceID);
		}
		public void Stop()
		{
			AL.SourceStop(SourceID);
		}

		public bool IsPlaying()
		{
			return (AL.GetSourceState(SourceID) == ALSourceState.Playing);
		}
		public bool IsPaused()
		{
			return (AL.GetSourceState(SourceID) == ALSourceState.Paused);
		}
		public bool IsStopped()
		{
			return (AL.GetSourceState(SourceID) == ALSourceState.Stopped);
		}

		public event EventHandler Stopped;
		internal void PerformStopped()
		{
			if (Stopped != null)
				Stopped(this, EventArgs.Empty);
		}
	}
}