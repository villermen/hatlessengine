using System;

namespace HatlessEngine
{
	public class NotLoadedException : Exception { }

	public class MusicAlreadyPlayingException : Exception { }

	public class ProtocolMismatchException : Exception 
	{
		public ProtocolMismatchException() { }

		public ProtocolMismatchException(string message) : base(message) { }
	}
}
