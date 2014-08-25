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
		private Sprite _Sprite;
		public Sprite Sprite
		{
			get { return _Sprite; }
			set
			{
				//set bound size to 1:1 scale with sprite when sprite is changed manually
				Bounds.Size = value.FrameSize;
				_Sprite = value;
			}
		}
		public int Depth;

		/// <summary>
		/// Occurs when the user has pressed the left mouse button while on the button bounds.
		/// </summary>
		public event EventHandler Pressed;

		/// <summary>
		/// Occurs when the user has pressed and released the left mouse button without leaving the button bounds in between.
		/// </summary>
		public event EventHandler Clicked;

		public GUIButton(Point position, Sprite sprite, int depth = 0)
		{
			Bounds = new Rectangle(position, sprite.FrameSize);
			_Sprite = sprite;
			Depth = depth;
		}

		public override void Step()
		{
			if (Bounds.IntersectsWith(Input.MousePosition))
			{
				//mouseover
				if (State == 0)
					State = 1;

				//pressed
				if (State == 1 && Input.IsPressed(Button.MouseLeft))
				{
					State = 2;
					if (Pressed != null)
						Pressed(this, EventArgs.Empty);
				}

				//clicked
				if (State == 2 && Input.IsReleased(Button.MouseLeft))
				{
					if (Clicked != null)
						Clicked(this, EventArgs.Empty);
					State = 1;	
				}
			}
			else
				State = 0;
		}

		public override void Draw()
		{
			Sprite.Draw(Bounds.Position, Bounds.Size / Sprite.FrameSize, State, 0f, Depth);
		}
	}
}