using System;

namespace HatlessEngine
{
    public class LogicalObject
    {
        internal virtual void BeforeStep() { }
        public virtual void Step() { }
        internal virtual void AfterStep() { }

        internal virtual void BeforeDraw(float stepProgress) { }
        public virtual void Draw(float stepProgress) { }
        internal virtual void AfterDraw(float stepProgress) { }

        internal virtual void AfterCreation() { }
    }
}
