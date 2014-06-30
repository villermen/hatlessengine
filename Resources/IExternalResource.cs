using System;

namespace HatlessEngine
{
	public interface IExternalResource : IResource
	{
		string Filename { get; }
		bool Loaded { get; }

		void Load();
		void Unload();
	}
}