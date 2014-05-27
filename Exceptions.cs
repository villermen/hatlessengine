using System;

namespace HatlessEngine
{
	public class NotLoadedException : Exception { }

    public class AlreadyLoadedException : Exception { }

    public class InvalidDeviceException : Exception { }

	public class ProtocolMismatchException : Exception 
	{
		public ProtocolMismatchException() { }

		public ProtocolMismatchException(string message) : base(message) { }
	}

	public class InvalidObjectTypeException : Exception 
	{
		public InvalidObjectTypeException() { }

		public InvalidObjectTypeException(string message) : base(message) { }
	}

    public class CannotUnseeException : Exception { }
}
