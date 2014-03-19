using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class ObjectBlueprint
    {
        public Type Type;
        public List<object> Arguments;

        public ObjectBlueprint(Type type, params object[] arguments)
        {
			if (!type.IsAssignableFrom(typeof(LogicalObject)))
				throw new ArgumentException("Type is not derived from LogicalObject");
            Type = type;
            Arguments = new List<object>(arguments);
        }

        public ObjectBlueprint(Type type) 
			: this(type, new object[0]) { }
    }
}
