using System;

namespace HatlessEngine
{
    public class Music
    {
        public string Filename { get; private set; }
        public string Id { get; internal set; }
        public bool IsLoaded { get; private set; }
        internal SFML.Audio.Music SFMLMusic;

        public Music(string filename)
        {
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
            SFMLMusic = null;
            IsLoaded = false;
        }
    }
}
