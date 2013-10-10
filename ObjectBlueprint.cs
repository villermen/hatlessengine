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
            Type = type;
            Arguments = new List<object>(arguments);
        }

        public ObjectBlueprint(Type type) : this(type, new object[0]) { }
    }
}
