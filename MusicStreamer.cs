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
                foreach (KeyValuePair<string, Music> pair in Resources.Music)
                {
                    Music music = pair.Value;

                    if (music.Streaming)
                    {
                        workNeedsDoing = true;

                        int buffersProcessed;
                        AL.GetSource(music.SourceId, ALGetSourcei.BuffersProcessed, out buffersProcessed);

                        if (music.JustStartedPlaying)
                        {
                            //will force filling the 2 still empty buffers and make sure the activebuffer is right afterwards
                            buffersProcessed = 2;
                            music.ActiveBufferId = 1;
                            music.JustStartedPlaying = false;
                        }

                        //if the music's done with a buffer, fill it again and append it
                        while (buffersProcessed > 0)
                        {
                            Log.Message("Processing buffer " + music.ActiveBufferId.ToString() + " for " + music.Id);

                            AL.SourceUnqueueBuffer(music.SourceId);

                            //fill the just released buffer
                            int requestedSamples = music.WaveReader.SampleRate / 2 * music.WaveReader.Channels;
                            int readSamples;
                            short[] waveData = music.WaveReader.ReadSamples(requestedSamples, out readSamples);
                            AL.BufferData(music.BufferIds[music.ActiveBufferId], music.WaveReader.ALFormat, waveData, readSamples * 2, music.WaveReader.SampleRate);

                            AL.SourceQueueBuffer(music.SourceId, music.BufferIds[music.ActiveBufferId]);

                            if (++music.ActiveBufferId == 3)
                                music.ActiveBufferId = 0;

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
                                        newMusic.SourceId = music.SourceId;
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
                Thread.Sleep(200); //cya in a fifth of a second!
            }
            Running = false;
        }
    }
}
