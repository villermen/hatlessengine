using System;

namespace HatlessEngine
{
    public class Music : IExternalResource
    {
        public string Filename { get; private set; }
        public string Id { get; private set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Audio.Music SFMLMusic;

        public Music(string id, string filename)
        {
            Id = id;
            Filename = filename;
            IsLoaded = false;
        }

        public void Play(float volume = 1)
        {
            if (!IsLoaded)
                Load();
            SFMLMusic.Volume = volume * 100;
            SFMLMusic.Play();
        }

        public void Load()
        {
            SFMLMusic = new SFML.Audio.Music(Filename);
            IsLoaded = true;
        }

        public void Unload()
        {
            SFMLMusic.Dispose();
            SFMLMusic = null;
            IsLoaded = false;
        }
    }
}
