using System;
using OpenTK.Audio.OpenAL;

namespace HatlessEngine
{
	/// <summary>
	/// Provides methods and events for controlling a specific instance of a sound playing.
	/// </summary>
	public sealed class SoundControl : AudioControl
	{
		internal SoundControl(int source)
		{
			Resources.AudioControls.Add(source, this);
			SourceID = source;
		}
	}
}

