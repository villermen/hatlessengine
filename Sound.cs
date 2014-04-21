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

        //default balance/volume?

		private int OpenALBufferId;

        private WaveReader.SoundFormat SoundFormat;
        private ALFormat ALFormat;
        private TimeSpan Duration;

        internal Sound(string id, string filename)
        {
            Id = id;
            Filename = filename;
            Loaded = false;
        }

		public SoundControl Play(float volume = 1f, float balance = 0f)
        {
			if (!Loaded)
			{
				if (Resources.JustInTimeLoading)
					Load();
				else
					throw new NotLoadedException();
			}

			//generate source and reference the buffer
			int source = Resources.GetSource();
			AL.Source(source, ALSourcei.Buffer, OpenALBufferId);

            SoundControl soundControl = new SoundControl(source);
            soundControl.Volume = volume;
            soundControl.Balance = balance;

			AL.SourcePlay(source);

			return soundControl;
        }

        public void Load()
		{
			if (!Loaded)
			{
				WaveReader waveReader = new WaveReader(Resources.GetStream(Filename));
				if (waveReader.MetaLoaded)
				{
					OpenALBufferId = AL.GenBuffer();
					int readSamples;
					short[] waveData = waveReader.ReadAll(out readSamples);
					AL.BufferData(OpenALBufferId, waveReader.ALFormat, waveData, waveData.Length * 2, waveReader.SampleRate);

                    SoundFormat = waveReader.Format;
                    ALFormat = waveReader.ALFormat;
                    Duration = waveReader.Duration;

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

        public override string ToString()
        {
            if (Loaded)
                return "'" + Id + "' (" + Filename + ", " + SoundFormat.ToString() + ", " + ALFormat.ToString() + ", " + Duration.ToString() + ")";
            else
                return "'" + Id + "' (" + Filename + ", Unloaded)";            
        }
    }
}
