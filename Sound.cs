using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	public class Sound : IExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool Loaded { get; private set; }

		private int OpenALBufferId;

        internal Sound(string id, string filename)
        {
            Id = id;
            Filename = filename;
            Loaded = false;
        }

		public SoundControl Play(float volume = 1f)
        {
			if (!Loaded)
			{
				if (Settings.JustInTimeResourceLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			//generate source and reference the buffer
			int source = Resources.GetSource();
			AL.Source(source, ALSourcei.Buffer, OpenALBufferId);

			if (volume != 1)
				AL.Source(source, ALSourcef.Gain, volume);

			AL.SourcePlay(source);

			SoundControl soundControl = new SoundControl(source);
			soundControl.Volume = volume;
			return soundControl;
        }

        public void Load()
		{
			if (!Loaded)
			{
				WaveReader reader = new WaveReader(Filename);
				if (reader.MetaLoaded)
				{
					OpenALBufferId = AL.GenBuffer();
					int readSamples;
					short[] waveData = reader.ReadAll(out readSamples);
					AL.BufferData(OpenALBufferId, reader.ALFormat, waveData, waveData.Length * 2, reader.SampleRate);
					Loaded = true;
				}
				else
					throw new FileLoadException();
			}
        }

        public void Unload()
        {
			if (Loaded)
			{
				AL.DeleteBuffer(OpenALBufferId);
				Loaded = false;
			}
        }
    }
}
