using System;

namespace HatlessEngine
{
    public static class Settings
    {
		/// <summary>
		/// If resource execution (like Music.Play or Sprite.Draw) is requested when it is loaded, instead of throwing an exception load it at that point.
		/// NOT A GREAT PRACTICE, use this only in very specific situations.
		/// </summary>
		public static bool JustInTimeResourceLoading = false;
    }
}
