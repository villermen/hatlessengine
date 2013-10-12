using System;

namespace HatlessEngine
{
    public class LogicalObject
    {
        private bool _Destroyed = false;
        public bool Destroyed
        { 
            get { return _Destroyed; }
            private set { _Destroyed = value; }
        }

        public LogicalObject()
        {
            //add object to Resource's objectlist
            Resources.AddObjects.Add(this);
        }

        public virtual void Step() { }
        internal virtual void AfterStep() { }

        public virtual void Draw(float stepProgress) { }
        internal virtual void AfterDraw(float stepProgress) { }

        public void Destroy()
        {
            //add for removal from Resources.Objects (cant be done now because of iteration)
            Resources.RemoveObjects.Add(this);
            Destroyed = true;
            OnDestroy();
        }
        public virtual void OnDestroy() { }
    }
}
