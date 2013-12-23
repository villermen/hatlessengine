using System;

namespace HatlessEngine
{
	public interface ExternalResource
    {
        string Id { get; }
        void Load();
        void Unload();
        bool Loaded { get; }
        string Filename { get; }

		//LoadMethod LoadMethod { get; set; }
    }
}
