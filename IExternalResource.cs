using System;

namespace HatlessEngine
{
	public interface IExternalResource
	{
		string ID { get; }
		void Load();
		void Unload();
		bool Loaded { get; }
		string Filename { get; }
	}
}