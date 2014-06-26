using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HatlessEngine
{
	internal static class MusicStreamer
	{
		public static bool Running = false;
		public static Thread Thread;

		public static void Launch()
		{
			if (!Running)
			{
				Thread = new Thread(new ThreadStart(Streamer));
				Thread.Name = "HatlessEngine MusicStreamer";
				Thread.IsBackground = true;
				Thread.Start();

				Running = true;
			}
		}

		private static void Streamer()
		{
			Running = true;

			//decides whether the thread should stay alive
			bool workNeedsDoing = true;

			while (workNeedsDoing)
			{
				workNeedsDoing = false;

				//work when there's workin' to do
				foreach (Music music in Resources.Music.Values)
				{
					//you no play, we no stream
					if (AL.GetSourceState(music.SourceID) != ALSourceState.Playing)
						music.Streaming = false;

					if (music.Streaming)
					{
						workNeedsDoing = true;

						int buffersProcessed;
						AL.GetSource(music.SourceID, ALGetSourcei.BuffersProcessed, out buffersProcessed);

						if (music.JustStartedPlaying)
						{
							//will force filling the 2 still empty buffers and make sure the activebuffer is right afterwards
							buffersProcessed = 2;
							music.ActiveBufferID = 1;
							music.JustStartedPlaying = false;
						}

						//if the music's done with a buffer, fill it again and append it
						while (buffersProcessed > 0)
						{
							AL.SourceUnqueueBuffer(music.SourceID);

							//fill the just released buffer with half a second of goodness
							int requestedSamples = music.WaveReader.SampleRate / 2 * music.WaveReader.Channels;
							int readSamples;
							short[] waveData = music.WaveReader.ReadSamples(requestedSamples, out readSamples);
							AL.BufferData(music.BufferIDs[music.ActiveBufferID], music.WaveReader.ALFormat, waveData, readSamples * 2, music.WaveReader.SampleRate);

							AL.SourceQueueBuffer(music.SourceID, music.BufferIDs[music.ActiveBufferID]);

							if (++music.ActiveBufferID == 3)
								music.ActiveBufferID = 0;

							//perform MusicChanged event in old music when it actually switched over
							if (music.PerformMusicChangedEventDelay != 3)
							{
								music.PerformMusicChangedEventDelay--;
								if (music.PerformMusicChangedEventDelay == 0)
								{
									music.PerformMusicChangedEventMusic.PerformMusicChanged(music);
									music.PerformMusicChangedEventMusic = null;
									music.PerformMusicChangedEventDelay = 3;
								}
							}

							//this was the last buffer, take action
							if (readSamples != requestedSamples)
							{
								if (music.Loop)
								{
									music.WaveReader.Rewind(music.LoopStartSample);
								}
								else
								{
									music.Streaming = false; //reached end

									//tight looping by hijacking sound source for new music
									if (music.PlayAfterMusic != "")
									{
										Music newMusic = Resources.Music[music.PlayAfterMusic];
										newMusic.SourceID = music.SourceID;
										newMusic.WaveReader.Rewind();
										newMusic.Streaming = true;

										//for having the MusicChanged event trigger at a more accurate time (still not perfect, but meh)
										newMusic.PerformMusicChangedEventDelay = 2;
										newMusic.PerformMusicChangedEventMusic = music;
									}
								}
							}

							buffersProcessed--;
						}
					}
				}
				Thread.Sleep(400); //get back before the buffers runs out will ya
			}
			Running = false;
		}
	}
}