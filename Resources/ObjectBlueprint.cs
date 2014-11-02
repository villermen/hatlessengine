using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	[Serializable]
	public struct ObjectBlueprint
	{
		public Type Type;
		public object[] Arguments;

		public ObjectBlueprint(Type type, params object[] arguments)
		{
			if (!type.IsSubclassOf(typeof(GameObject)))
				throw new ArgumentException("Type is not derived from LogicalObject");
			Type = type;
			Arguments = arguments;
		}
	}
}