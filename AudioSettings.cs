using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;

namespace HatlessEngine
{
	public static class AudioSettings
	{
		private static string _CurrentPlaybackDevice = AudioContext.DefaultDevice;
		public static string CurrentPlaybackDevice 
		{ 
			get { return _CurrentPlaybackDevice; }
		}

		/// <summary>
		/// Gets all devices available for playback.
		/// (Devices are identified by their names.)
		/// Currently does not change during runtime.
		/// </summary>
		public static IList<string> AvailablePlaybackDevices
		{
			get { return AudioContext.AvailableDevices; }
		}

		/// <summary>
		/// Sets the playback device to the given one.
		/// Will stop all playing audio and will reload all already loaded music and sounds because of how OpenAL works. (you usually don't want to do this on-the-fly anyway right?)
		/// </summary>
		public static void SetPlaybackDevice(string deviceName)
		{
			if (!AvailablePlaybackDevices.Contains(deviceName))
				throw new InvalidDeviceException();
			
			Game.AudioContext = new AudioContext(deviceName, 0, 0, false, false);
			Game.AudioContext.MakeCurrent();

			AL.DistanceModel(ALDistanceModel.None);

			//reload all previously loaded sounds and music (new context, new buffers)
			foreach(Sound sound in Resources.Sounds.Values)
			{
				if (sound.Loaded)
					sound.LoadForced();
			}
			foreach (Music music in Resources.Music.Values)
			{
				if (music.Loaded)
					music.LoadForced();
			}

			_CurrentPlaybackDevice = deviceName;
		}
		/// <summary>
		/// Sets the playback device to the system default.
		/// </summary>
		public static void SetPlaybackDevice()
		{
			SetPlaybackDevice(AudioContext.DefaultDevice);
		}
	}
}