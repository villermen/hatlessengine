using System;

namespace HatlessEngine
{
	/// <summary>
	/// Class using LogicalObject to easily define buttons.
	/// Use a sprite with at least 3 frames where:
	/// Frame 0 = idle
	/// Frame 1 = mouseover
	/// Frame 2 = mousedown
	/// </summary>
	public class GUIButton : LogicalObject
	{
		/// <summary>
		/// Current state of the button
		/// 0: nothing, 1: mouseover, 2: mousedown
		/// </summary>
		public int State { get; private set; }
		public Rectangle Bounds;
		/// <summary>
		/// Code to run when button is pressed and released without the mouse leaving it.
		/// </summary>
		public Action OnClick;

		private Sprite Sprite;
		
		private int Depth;

		public GUIButton(Point position, Sprite sprite, Action onClick, int depth = 0)
		{
			Sprite = sprite;
			Bounds = new Rectangle(position, sprite.FrameSize);
			OnClick = onClick;
			Depth = depth;
		}

		public sealed override void Step()
		{
			if (Bounds.IntersectsWith(Input.MousePosition))
			{
				//mouseover
				if (State == 0)
					State = 1;

				//pressed
				if (State == 1 && Input.IsPressed(Button.MouseLeft))
					State = 2;

				//clicked
				if (State == 2 && Input.IsReleased(Button.MouseLeft))
				{
					OnClick.Invoke();
					State = 1;
				}
			}
			else
				State = 0;
		}

		public sealed override void Draw()
		{
			Sprite.Draw(Bounds.Position, State, 0f, Depth);
		}
	}
}