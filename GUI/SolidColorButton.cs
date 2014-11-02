using System;

namespace HatlessEngine.GUI
{
	/// <summary>
	/// A button with solid colored backgrounds and text. Useful for quick button creation without a design.
	/// </summary>
	public class SolidColorButton : TextButton
	{
		public Color DefaultColor;
		public Color MouseoverColor;
		public Color MousedownColor;

		private Color CurrentColor;

		public SolidColorButton(Rectangle bounds, string text, Font textFont, Color textColor, Color defaultColor, Color mouseoverColor, Color mousedownColor, Alignment textAlignment = Alignment.CenterMiddle, int depth = 0)
			: base(bounds, text, textFont, textColor, textAlignment, depth)
		{
			DefaultColor = defaultColor;
			MouseoverColor = mouseoverColor;
			MousedownColor = mousedownColor;
		}
		public SolidColorButton(Rectangle bounds, string text, Font textFont, Alignment textAlignment = Alignment.CenterMiddle, int depth = 0)
			: this(bounds, text, textFont, Color.Black, Color.Silver, Color.Gray, Color.Lime, textAlignment, depth) { }
		public SolidColorButton(Rectangle bounds, int depth = 0)
			: this(bounds, null, null, Color.Black, Color.Silver, Color.Gray, Color.Lime, Alignment.CenterMiddle, depth) { }

		public override void Step()
		{
			base.Step();

			if (State == ButtonState.Mouseover)
				CurrentColor = MouseoverColor;
			else if (State == ButtonState.Mousedown)
				CurrentColor = MousedownColor;
			else
				CurrentColor = DefaultColor;
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(Bounds, CurrentColor, Depth);

			base.Draw();
		}
	}
}
