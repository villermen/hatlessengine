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

		private Color _currentColor;

		public SolidColorButton(Rectangle bounds, string text, Font textFont, Color textColor, Color defaultColor, Color mouseoverColor, Color mousedownColor, CombinedAlignment textAlignment = CombinedAlignment.MiddleCenter, int depth = 0)
			: base(bounds, text, textFont, textColor, textAlignment, depth)
		{
			DefaultColor = defaultColor;
			MouseoverColor = mouseoverColor;
			MousedownColor = mousedownColor;
		}
		public SolidColorButton(Rectangle bounds, string text, Font textFont, CombinedAlignment textAlignment = CombinedAlignment.MiddleCenter, int depth = 0)
			: this(bounds, text, textFont, Color.Black, Color.Silver, Color.Gray, Color.Lime, textAlignment, depth) { }
		public SolidColorButton(Rectangle bounds, int depth = 0)
			: this(bounds, null, null, Color.Black, Color.Silver, Color.Gray, Color.Lime, CombinedAlignment.MiddleCenter, depth) { }

		public override void Step()
		{
			base.Step();

			if (State == ButtonState.Mouseover)
				_currentColor = MouseoverColor;
			else if (State == ButtonState.Mousedown)
				_currentColor = MousedownColor;
			else
				_currentColor = DefaultColor;
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(Bounds, _currentColor, Depth);

			base.Draw();
		}
	}
}
