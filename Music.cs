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

		internal WaveReader WaveReader;

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

		public void Play(float initialVolume = 1f)
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

			if (initialVolume != 1)
				AL.Source(SourceId, ALSourcef.Gain, initialVolume);

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
    }
}
