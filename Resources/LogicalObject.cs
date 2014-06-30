using System;

namespace HatlessEngine
{
	public class LogicalObject
	{
		public bool Destroyed { get; private set; }

		public LogicalObject()
		{
			//add object to Resource's objectlist
			Resources.AddObjects.Add(this);
		}

		public virtual void Step() { }

		public virtual void Draw() { }

		public void Destroy()
		{
			OnDestroy();
			//add for removal from Resources.Objects (cant be done now because Game is looping through it at the moment)
			Resources.RemoveObjects.Add(this);
			Destroyed = true;
		}
		public virtual void OnDestroy() { }
	}
}