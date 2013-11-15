using System;

namespace HatlessEngine
{
    interface IExternalResource
    {
        string Id { get; }
        void Load();
        void Unload();
        bool IsLoaded { get; }
        string Filename { get; }

        /* StorageMethod, enum, stream or filename, more if needed */
    }
}
