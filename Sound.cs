using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class Sound
    {
        public string Filename { get; private set; }
        public string Id { get; internal set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Audio.SoundBuffer SFMLSoundBuffer;
        private Dictionary<byte, SFML.Audio.Sound> SFMLSounds = new Dictionary<byte, SFML.Audio.Sound>(256);
        private byte NextId = 0;

        public Sound(string filename)
        {
            Filename = filename;
            IsLoaded = false;
        }

        public byte Play(float volume = 1)
        {
            if (!IsLoaded)
                Load();

            SFMLSounds.Add(NextId, new SFML.Audio.Sound(SFMLSoundBuffer));
            SFMLSounds[NextId].Volume = volume * 100;
            SFMLSounds[NextId].Play();

            NextId++;

            return (byte)(NextId - 1);
        }

        public void Load()
        {
            SFMLSoundBuffer = new SFML.Audio.SoundBuffer(Filename);
            IsLoaded = true;
        }

        public void Unload()
        {
            SFMLSoundBuffer = null;
            IsLoaded = false;
        }
    }
}
