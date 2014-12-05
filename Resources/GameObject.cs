namespace HatlessEngine
{
	/// <summary>
	/// Represents an object without any built-in physical support.
	/// Use this for objects that don't need to use the built-in support for moveing or collision detection.
	/// </summary>
	public abstract class GameObject
	{
		public bool Destroyed { get; private set; }

		protected GameObject()
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