using System;

namespace HatlessEngine
{
	[Serializable]
	public class NotLoadedException : Exception { }

	[Serializable]
	public class InvalidDeviceException : Exception { }

	[Serializable]
	public class ProtocolMismatchException : Exception 
	{
		public ProtocolMismatchException() { }

		public ProtocolMismatchException(string message) : base(message) { }
	}

	[Serializable]
	public class InvalidObjectTypeException : Exception
	{
		public InvalidObjectTypeException() { }

		public InvalidObjectTypeException(string message) : base(message) { }
	}

	[Serializable]
	public class NonConvexShapeDesignException : Exception { }

	[Serializable]
	public class IndexNotFoundException : Exception
	{
		public IndexNotFoundException() { }

		public IndexNotFoundException(string message) : base(message) { }
	}
}