using System;
using System.Diagnostics;
using System.Reflection;

namespace HatlessEngine
{
	/// <summary>
	/// Base class for resources that have a loadable/disposable memory object associated with it.
	/// </summary>
	public abstract class ExternalResource : Resource, IDisposable
	{
		public string File { get; private set; }
		protected Assembly FileAssembly { get; set; }
		public bool Loaded { get; protected set; }

		protected ExternalResource(string id, string file)
			: base(id)
		{
			File = file;

			//get assembly of code instantiating a derived object
			StackTrace stackTrace = new StackTrace();
			for(int frame = 2; frame < stackTrace.FrameCount; frame++)
			{
				Type frameType = stackTrace.GetFrame(frame).GetMethod().DeclaringType;

				if (frameType.IsSubclassOf(typeof(ExternalResource))) 
					continue;

				FileAssembly = frameType.Assembly;
				break;
			}

			Log.Message(FileAssembly.ToString());

			if (FileAssembly == null)
				throw new Exception("Not instantialized from outside ExternalResource.");
			
			Loaded = false;

			//add to external resource list
			Resources.ExternalResources.Add(this);
		}

		public abstract void Load();
		public abstract void Unload();

		/// <summary>
		/// Unload and remove the resource entirely.
		/// </summary>
		public override void Destroy()
		{
			Unload();
			base.Destroy();

			Resources.ExternalResources.Remove(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			//destroy either way, this overload is just for convention
			Destroy();
		}

		/// <summary>
		/// Pretty much an alias for Destroy(), here just to implement IDisposable as this object uses unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			//do not suppress recommended finalization as the resource could be loaded after this point
		}

		~ExternalResource()
		{
			Dispose(false);
		}
	}
}
