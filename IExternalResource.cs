using System;

namespace HatlessEngine
{
	public interface IExternalResource
    {
        string Id { get; }
        void Load();
        void Unload();
        bool Loaded { get; }
        string Filename { get; }

		//LoadMethod LoadMethod { get; set; }
    }
}
