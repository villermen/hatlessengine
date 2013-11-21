using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class Sound : IExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Audio.SoundBuffer SFMLSoundBuffer;
        private Dictionary<byte, SFML.Audio.Sound> SFMLSounds = new Dictionary<byte, SFML.Audio.Sound>(256);
        private byte NextId = 0;

        internal Sound(string id, string filename)
        {
            Id = id;
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
            SFMLSoundBuffer.Dispose();
            SFMLSoundBuffer = null;
            IsLoaded = false;
        }
    }
}
