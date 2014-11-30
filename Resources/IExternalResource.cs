using System.Reflection;

namespace HatlessEngine
{
	public interface IExternalResource : IResource
	{
		string Filename { get; }
		Assembly FileAssembly { get; }
		bool Loaded { get; }

		void Load();
		void Unload();
	}
}