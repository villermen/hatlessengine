using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	/// <summary>
	/// Provides methods and events for controlling a specific instance of a sound playing.
	/// </summary>
	public class SoundControl : AudioControl
	{
		internal SoundControl(int source)
		{
			Resources.AudioControls.Add(source, this);
			SourceId = source;
		}
	}
}

