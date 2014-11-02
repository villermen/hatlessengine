using System;

namespace HatlessEngine.GUI
{
	/// <summary>
	/// A button with sprited backgrounds. The sprite will be stretched to fill Bounds.
	/// </summary>
	public class FullImageButton : TextButton
	{
		/// <summary>
		/// A sprite with 3 equal-sized frames.
		/// default - hover - mousedown
		/// </summary>
		public Sprite Sprite;

		public FullImageButton(Rectangle bounds, Sprite sprite, string text, Font textFont, Color textColor, Alignment textAlignment = Alignment.CenterMiddle, int depth = 0)
			: base(bounds, text, textFont, textColor, textAlignment, depth)
		{
			Sprite = sprite;		
		}
		public FullImageButton(Point position, Sprite sprite, string text, Font textFont, Color textColor, Alignment textAlignment = Alignment.CenterMiddle, int depth = 0)
			: this(new Rectangle(position, sprite.FrameSize), sprite, text, textFont, textColor, textAlignment, depth) { }
		public FullImageButton(Rectangle bounds, Sprite sprite, int depth = 0)
			: this(bounds, sprite, null, null, Color.Black, Alignment.CenterMiddle, depth) { }
		public FullImageButton(Point position, Sprite sprite, int depth = 0)
			: this(new Rectangle(position, sprite.FrameSize), sprite, null, null, Color.Black, Alignment.CenterMiddle, depth) { }

		public override void Draw()
		{
			Sprite.Draw(Bounds.Position, Bounds.Size / Sprite.FrameSize, (int)State, 0f, Depth);

			base.Draw();
		}
	}
}