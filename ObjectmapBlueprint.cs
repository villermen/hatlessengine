using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class ObjectmapBlueprint
    {
        public Type Type;
        public List<object> Arguments;

        public ObjectmapBlueprint(Type type, params object[] arguments)
        {
            Type = type;
            Arguments = new List<object>(arguments);
        }

        public ObjectmapBlueprint(Type type) 
			: this(type, new object[0]) { }
    }
}
