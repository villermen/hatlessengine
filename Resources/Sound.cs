using System;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{/*
	public sealed class Sound : IExternalResource
	{
		public string ID { get; private set; }
		public string Filename { get; private set; }
		public bool Loaded { get; private set; }

		//default balance/volume?

		private int OpenALBufferID;

		private SoundDataFormat SoundDataFormat;
		private ALFormat ALFormat;
		private TimeSpan Duration;

		public Sound(string id, string filename)
		{
			ID = id;
			Filename = filename;
			Loaded = false;

			Resources.Sounds.Add(ID, this);
			Resources.ExternalResources.Add(this);
		}

		public SoundControl Play(float volume = 1f, float balance = 0f)
		{
			if (!Loaded)
				throw new NotLoadedException();

			//generate source and reference the buffer
			int source = Resources.GetSource();
			AL.Source(source, ALSourcei.Buffer, OpenALBufferID);

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
				using (WaveReader waveReader = new WaveReader(Resources.GetStream(Filename)))
				{
					if (waveReader.MetaLoaded)
					{
						OpenALBufferID = AL.GenBuffer();
						int readSamples;
						short[] waveData = waveReader.ReadAll(out readSamples);
						AL.BufferData(OpenALBufferID, waveReader.ALFormat, waveData, waveData.Length * 2, waveReader.SampleRate);

						SoundDataFormat = waveReader.Format;
						ALFormat = waveReader.ALFormat;
						Duration = waveReader.Duration;

						Loaded = true;
					}
					else
						throw new FileLoadException();
				}
			}
		}

		/// <summary>
		/// For rearming after AudioContext has been destroyed.
		/// </summary>
		internal void LoadForced()
		{
			Loaded = false;
			Load();
		}

		public void Unload()
		{
			if (Loaded)
			{
				AL.DeleteBuffer(OpenALBufferID);
				OpenALBufferID = 0;
				Loaded = false;
			}
		}

		public override string ToString()
		{
			if (Loaded)
				return "'" + ID + "' (" + Filename + ", " + SoundDataFormat.ToString() + ", " + ALFormat.ToString() + ", " + Duration.ToString() + ")";
			else
				return "'" + ID + "' (" + Filename + ", Unloaded)";			
		}

		public void Destroy()
		{
			Unload();

			Resources.Sounds.Remove(ID);
			Resources.ExternalResources.Remove(this);
		}
	}*/
}