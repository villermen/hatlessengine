using System;

namespace HatlessEngine
{
    public class LogicalObject
    {
        public virtual void Step() { }

        public virtual void Draw(float stepProgress) { }

        public virtual void OnCreate() { }
    }
}
