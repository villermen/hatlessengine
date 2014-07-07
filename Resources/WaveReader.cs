using System;
using System.IO;

namespace HatlessEngine
{
	/*
	/// <summary>
	/// Reads OpenAL compatible wavedata from audiofiles.
	/// </summary>
	internal class WaveReader : IDisposable
	{
		public bool MetaLoaded = false;
		public SoundDataFormat Format;

		public int BitsPerSample;

		public int SampleRate;
		public short Channels;

		public long WaveSampleStartPosition;
		public int WaveTotalSamples;
		
		public ALFormat ALFormat;

		private BinaryReader Reader;
		private VorbisReader VorbisReader;

		public TimeSpan Duration;

		public WaveReader(BinaryReader stream)
		{
			Reader = stream;

			string signature = new string(Reader.ReadChars(4));

			if (signature == "RIFF") //Wave
			{
				Reader.BaseStream.Seek(4, SeekOrigin.Current);
				//int riffChunkSize = reader.ReadInt32();

				signature = new string(Reader.ReadChars(4));

				if (signature == "WAVE")
				{
					Format = SoundDataFormat.Wave;

					signature = new string(Reader.ReadChars(4));
					if (signature == "fmt ")
					{
						Reader.BaseStream.Seek(4 + 2, SeekOrigin.Current);
						//int format_chunk_size = reader.ReadInt32();
						//int audio_format = reader.ReadInt16();

						Channels = Reader.ReadInt16();
						SampleRate = Reader.ReadInt32();

						Reader.BaseStream.Seek(4 + 2, SeekOrigin.Current);
						//int byte_rate = reader.ReadInt32();
						//int block_align = reader.ReadInt16();

						BitsPerSample = Reader.ReadInt16();

						signature = new string(Reader.ReadChars(4));
						if (signature == "data")
						{
							WaveSampleStartPosition = Reader.BaseStream.Position;

							WaveTotalSamples = Reader.ReadInt32() / (BitsPerSample / 8);
						
							//decide format for AL to use
							if (Channels == 1) //mono
							{
								if (BitsPerSample == 8)
									ALFormat = ALFormat.Mono8;
								else
									ALFormat = ALFormat.Mono16;
							} else
							{
								if (BitsPerSample == 8)
									ALFormat = ALFormat.Stereo8;
								else
									ALFormat = ALFormat.Stereo16;
							}

							Duration = new TimeSpan(0, 0, 0, 0, (int)Math.Ceiling((float)WaveTotalSamples / (float)SampleRate / (float)Channels * 1000f));

							MetaLoaded = true;
						}
					}
				}
			} 
			else if (signature == "OggS") //Vorbis / Ogg Sound
			{
				Format = SoundDataFormat.Vorbis;

				VorbisReader = new VorbisReader(Reader.BaseStream, true);

				BitsPerSample = 16;
				SampleRate = VorbisReader.SampleRate;
				Channels = (short)VorbisReader.Channels;

				if (Channels == 1)
					ALFormat = ALFormat.Mono16;
				else
					ALFormat = ALFormat.Stereo16;

				Duration = VorbisReader.TotalTime;

				MetaLoaded = true;
			}
			else
				Format = SoundDataFormat.Unsupported;
		}

		public short[] ReadAll(out int readSamples)
		{
			if (Format == SoundDataFormat.Wave)
			{
				return ReadSamples(WaveTotalSamples, out readSamples);
			}
			else if (Format == SoundDataFormat.Vorbis)
			{
				return ReadSamples((int)VorbisReader.TotalSamples, out readSamples);
			}
			else
				throw new NotSupportedException("Not Wave or Vorbis.");
		}

		public short[] ReadSamples(int samples, out int readSamples)
		{
			if (Format == SoundDataFormat.Wave)
			{
				byte[] byteBuffer = Reader.ReadBytes(BitsPerSample / 8 * samples);
				readSamples = byteBuffer.Length / 2;

				short[] waveData = new short[readSamples];
				for(int i = 0; i < waveData.Length; i++)
				{
					waveData[i] = BitConverter.ToInt16(byteBuffer, i * 2);
				}
				return waveData;
			}
			else if (Format == SoundDataFormat.Vorbis)
			{
				float[] floatBuffer = new float[samples];
				readSamples = VorbisReader.ReadSamples(floatBuffer, 0, samples);

				short[] waveData = new short[readSamples];
				for(int i = 0; i < readSamples; i++)
				{
					waveData[i] = (short)(32767f * floatBuffer[i]);
				}

				return waveData;
			} 
			else
				throw new NotSupportedException("Not Wave or Vorbis.");
		}

		/// <summary>
		/// Rewind to a position (0 for start)
		/// </summary>
		/// <param name="samples">Samples.</param>
		public void Rewind(uint samples = 0)
		{
			if (MetaLoaded)
			{
				if (Format == SoundDataFormat.Wave)
					Reader.BaseStream.Position = WaveSampleStartPosition + BitsPerSample / 8 * samples * Channels;
				else if (Format == SoundDataFormat.Vorbis)
					VorbisReader.DecodedPosition = samples;
			}
		}

		~WaveReader()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Reader != null)
				{
					Reader.Dispose();
					Reader = null;
				}
				if (VorbisReader != null)
				{
					VorbisReader.Dispose();
					VorbisReader = null;
				}
			}
		}
	}

	internal enum SoundDataFormat
	{
		Unsupported = 0,
		Wave = 1,
		Vorbis = 2
	}
	*/
}