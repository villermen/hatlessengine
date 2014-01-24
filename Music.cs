using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	public class Music :  AudioControl, ExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool Loaded { get; private set; }

		public bool Loop = false; //implement in audiocontrol

		internal bool Streaming = false;

		internal int[] BufferIds;
		internal byte ActiveBufferId;

		internal bool JustStartedPlaying = false;

		//for triggering event in old music when this one is set as the PlayAfter one (needs to be triggered on a delay)
		internal byte PerformMusicChangedEventDelay = 3;
		internal Music PerformMusicChangedEventMusic = null;

		internal WaveReader WaveReader;

		/// <summary>
		/// Sample to start after when this music is looped.
		/// One sample is both channels for stereo. 
		/// Audacity is a good way to find out what sample you need.
		/// Just split the files into an intro and loop region if you don't want to use this.
		/// </summary>
		public uint LoopStartSample = 0;

		/// <summary>
		/// Music to play directly after this one ends.
		/// Of course this doesn't work when this one's in looping mode.
		/// </summary>
		public string PlayAfterMusic = "";

		internal Music(string id, string filename)
        {
            Id = id;
            Filename = filename;
            Loaded = false;
        }

		public void Play()
        {
			if (!Loaded)
			{
				if (Settings.JustInTimeResourceLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			if (Streaming)
			{
				throw new AlreadyPlayingException();
			}

			WaveReader.Rewind();

			SourceId = Resources.GetSource();

			//fill first buffer to make sure the music can start playing after this point
			int readSamples;
			short[] waveData = WaveReader.ReadSamples(WaveReader.SampleRate / 2 * WaveReader.Channels, out readSamples);
			AL.BufferData(BufferIds[0], WaveReader.ALFormat, waveData, readSamples * 2, WaveReader.SampleRate);

			//attach
			AL.SourceQueueBuffer(SourceId, BufferIds[0]);

			if (_Volume != 1)
				AL.Source(SourceId, ALSourcef.Gain, _Volume);

			AL.SourcePlay(SourceId);

			JustStartedPlaying = true;
			Streaming = true;

			Resources.LaunchMusicStreamerThread();

			Resources.AudioSources.Add(SourceId);
			Resources.AudioControls.Add(SourceId, this);
        }

        public void Load()
        {
			if (!Loaded)
			{
				WaveReader = new WaveReader(Filename);
				if (WaveReader.MetaLoaded)
					Loaded = true;
				BufferIds = AL.GenBuffers(3);
			}
        }

        public void Unload()
        {
			if (Loaded)
			{
				WaveReader.Dispose();
				WaveReader = null;
				AL.DeleteBuffers(BufferIds);
				Loaded = false;
			}
        }

		/// <summary>
		/// Occurs when this music is done playing and it's switching over the source to the given PlayAfterMusic.
		/// </summary>
		public event EventHandler<MusicChangedEventArgs> MusicChanged;

		internal void PerformMusicChanged(Music newMusic)
		{
			if (MusicChanged != null)
				MusicChanged(this, new MusicChangedEventArgs(this, newMusic));
		}
    }
}
