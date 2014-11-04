using System;

namespace HatlessEngine
{
	[Serializable]
	public class View : IResource
	{
		public string ID { get; private set; }

		/// <summary>
		/// Whether this View is drawn to the window.
		/// </summary>
		public bool Active;

		/// <summary>
		/// <para>The area represented by this view.</para>
		/// <para>Everything inside this area will be scaled to fit inside the Viewport.</para>
		/// <para>Position is always absolute.</para>
		/// <para>Size can be a fraction of the window size if &lt;= 1f, else it&apos;s absolute.</para>
		/// <para>Both directions are treated individually so one can be absolute while the other is relative.</para>
		/// <para>You can use negative values for the size to specify the factor of the area that the window has left (-2f = (window width - viewport x) * 2f)</para>
		/// </summary>
		public Rectangle GameArea;
		/// <summary>
		/// <para>The Window-area covered by this view.</para>
		/// <para>Everything inside GameArea will be scaled to fit in this area.</para>
		/// <para>Position and Size can both be a fraction of the window if their values are &lt;= 1f, or absolute if &gt; 1f.</para>
		/// <para>Both directions are treated individually so one can be absolute while the other is relative.</para>
		/// <para>You can use negative values for the size to specify the factor of the area that the window has left (-2f = (window width - viewport x) * 2f)</para>
		/// </summary>
		public Rectangle Viewport;

		/// <summary>
		/// See individual fields for explanation on what they are and how to use them.
		/// </summary>
		public View(string id, Rectangle area, Rectangle viewport, bool active = true)
		{
			ID = id;
			GameArea = area;
			Viewport = viewport;
			Active = active;

			Resources.Views.Add(ID, this);
		}

		/// <summary>
		/// Returns the game area in absolute coordinates, with size components based on the window's current size if set relatively.
		/// </summary>
		public Rectangle GetAbsoluteGameArea()
		{
			float absoluteWidth, absoluteHeight;
			Rectangle absoluteViewport = GetAbsoluteViewport();

			if (GameArea.Size.X > 1f)
				absoluteWidth = GameArea.Size.X;
			else if (GameArea.Size.X < 0f)
				absoluteWidth = (Window.Size.X - absoluteViewport.Position.X) * -GameArea.Size.X;
			else
				absoluteWidth = GameArea.Size.X * Window.Size.X;

			if (GameArea.Size.Y > 1f)
				absoluteHeight = GameArea.Size.Y;
			else if (GameArea.Size.Y < 0f)
				absoluteHeight = (Window.Size.Y - absoluteViewport.Position.Y) * -GameArea.Size.Y;
			else
				absoluteHeight = GameArea.Size.Y * Window.Size.Y;

			return new Rectangle(GameArea.Position, absoluteWidth, absoluteHeight);
		}

		/// <summary>
		/// Returns the viewport in absolute coordinates.
		/// </summary>
		public Rectangle GetAbsoluteViewport()
		{
			float absoluteX, absoluteY, absoluteWidth, absoluteHeight;

			if (Viewport.Position.X <= 1f)
				absoluteX = Viewport.Position.X * Window.Size.X;
			else
				absoluteX = Viewport.Position.X;

			if (Viewport.Position.Y <= 1f)
				absoluteY = Viewport.Position.Y * Window.Size.Y;
			else
				absoluteY = Viewport.Position.Y;

			if (Viewport.Size.X > 1f)
				absoluteWidth = Viewport.Size.X;
			else if (Viewport.Size.X < 0f)
				absoluteWidth = (Window.Size.X - absoluteX) * -Viewport.Size.X;
			else
				absoluteWidth = Viewport.Size.X * Window.Size.X;

			if (Viewport.Size.Y > 1f)
				absoluteHeight = Viewport.Size.Y;
			else if (Viewport.Size.Y < 0f)
				absoluteHeight = (Window.Size.Y - absoluteY) * -Viewport.Size.Y;
			else
				absoluteHeight = Viewport.Size.Y * Window.Size.Y;

			return new Rectangle(absoluteX, absoluteY, absoluteWidth, absoluteHeight);
		}

		/// <summary>
		/// The scale conversion from GameArea size to Viewport size.
		/// </summary>
		public Point GetScale()
		{
			return GetAbsoluteViewport().Size / GetAbsoluteGameArea().Size;
		}

		public void Destroy()
		{
			Resources.Views.Remove(ID);
		}

		public static implicit operator View(string str)
		{
			return Resources.Views[str];
		}
	}
}