using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    public class ObjectMapBlueprint
    {
        public Type Type;
        public List<object> Arguments;

        public ObjectMapBlueprint(Type type, params object[] arguments)
        {
            Type = type;
            Arguments = new List<object>(arguments);
        }

        public ObjectMapBlueprint(Type type) : this(type, new object[0]) { }
    }
}
