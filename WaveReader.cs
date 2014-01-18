using System;
using NVorbis;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	/// <summary>
	/// Reads OpenAL compatible wavedata from audiofiles.
	/// </summary>
	internal class WaveReader
	{
		public enum SoundFormat { 
			Unsupported = 0,
			Wave = 1,
			Ogg = 2 }

		public bool MetaLoaded = false;
		public SoundFormat Format;

		public int BitsPerSample;

		public int SampleRate;
		public short Channels;

		public long WaveSampleStartPosition;
		public int WaveTotalSamples;
		
		public ALFormat ALFormat;

		private BinaryReader Reader;
		private VorbisReader VorbisReader;

		public WaveReader(string filename)
		{
			Reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
			string signature = new string(Reader.ReadChars(4));

			if (signature == "RIFF") //Wave
			{
				Reader.BaseStream.Seek(4, SeekOrigin.Current);
				//int riffChunkSize = reader.ReadInt32();

				signature = new string(Reader.ReadChars(4));

				if (signature == "WAVE")
				{
					Format = SoundFormat.Wave;

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

							MetaLoaded = true;
						}
					}
				}
			} 
			else if (signature == "OggS") //Ogg
			{
				Format = SoundFormat.Ogg;

				VorbisReader = new VorbisReader(Reader.BaseStream, true);

				BitsPerSample = 16;
				SampleRate = VorbisReader.SampleRate;
				Channels = (short)VorbisReader.Channels;

				if (Channels == 1)
					ALFormat = ALFormat.Mono16;
				else
					ALFormat = ALFormat.Stereo16;

				MetaLoaded = true;
			}
			else
				Format = SoundFormat.Unsupported;
		}

		public short[] ReadAll(out int readSamples)
		{
			if (Format == SoundFormat.Wave)
			{
				return ReadSamples(WaveTotalSamples, out readSamples);
			}
			else if (Format == SoundFormat.Ogg)
			{
				return ReadSamples((int)VorbisReader.TotalSamples, out readSamples);
			}
			else
				throw new NotSupportedException("Not Wave or Ogg.");
		}

		public short[] ReadSamples(int samples, out int readSamples)
		{
			if (Format == SoundFormat.Wave)
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
			else if (Format == SoundFormat.Ogg)
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
				throw new NotSupportedException("Not Wave or Ogg.");
		}

		/// <summary>
		/// Rewind to a position (0 for start)
		/// </summary>
		/// <param name="samples">Samples.</param>
		public void Rewind(uint samples = 0)
		{
			if (MetaLoaded)
			{
				if (Format == SoundFormat.Wave)
					Reader.BaseStream.Position = WaveSampleStartPosition + BitsPerSample / 8 * samples * Channels;
				else if (Format == SoundFormat.Ogg)
					VorbisReader.DecodedPosition = samples;
			}
		}

		~WaveReader()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (Reader != null)
				Reader.Dispose();
			if (VorbisReader != null)
				VorbisReader.Dispose();
		}
	}
}

