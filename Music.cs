using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	public class Music :  AudioControl, ExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool Loaded { get; private set; }

		public bool Looping { get; private set; } //implement in audiocontrol

		internal bool Streaming = false;

		internal int[] BufferIds;
		internal byte ActiveBufferId;

		internal bool JustStartedPlaying = false;

		internal WaveReader WaveReader;

		internal Music(string id, string filename)
        {
            Id = id;
            Filename = filename;
            Loaded = false;
        }

		public void Play(float volume = 1f, bool looping = false)
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

			if (volume != 1)
				AL.Source(SourceId, ALSourcef.Gain, volume);

			AL.SourcePlay(SourceId);

			JustStartedPlaying = true;
			Streaming = true;
			Looping = looping;

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
