using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Base class for logical resources.
	/// </summary>
	public abstract class Resource
	{
		public readonly string Id;

		protected Resource(string id)
		{
			Id = id;

			//add to collection
			if (!Resources.Collection.ContainsKey(GetType()))
				Resources.Collection.Add(GetType(), new Dictionary<string, Resource>());

			Resources.Collection[GetType()].Add(Id, this);
		}

		/// <summary>
		/// Removes the resource entirely.
		/// </summary>
		public virtual void Destroy()
		{
			//remove from collection
			Resources.Collection[GetType()].Remove(Id);
		}
	}
}
