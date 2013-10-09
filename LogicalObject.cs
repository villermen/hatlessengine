using System;

namespace HatlessEngine
{
    public class LogicalObject
    {
        public LogicalObject()
        {
            //add object to Resource's objectlist
            Resources.Objects.Add(this);
        }

        public virtual void Step() { }
        internal virtual void AfterStep() { }

        public virtual void Draw(float stepProgress) { }
        internal virtual void AfterDraw(float stepProgress) { }
    }
}
