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
			    AL.Source(SourceId, ALSourcef.Gain, value);
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
                OpenTK.Vector3 position = new OpenTK.Vector3(value, 0f, (float)Math.Sqrt(1 - Math.Pow(value, 2))); //Thanks to Ethan Lee from FNA
                AL.Source(SourceId, ALSource3f.Position, ref position);
                _Balance = value;
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
