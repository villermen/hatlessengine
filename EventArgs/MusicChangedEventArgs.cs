using System;

namespace HatlessEngine
{
	public class MusicChangedEventArgs : EventArgs
	{
		public Music NewMusic;

		public MusicChangedEventArgs(Music newMusic)
		{
			NewMusic = newMusic;
		}
	}
}

