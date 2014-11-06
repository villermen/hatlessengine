using System;
using System.Text;

namespace HatlessEngine.GUI
{
	/// <summary>
	/// A single-line text input that will scroll horizontally until max characters is reached.
	/// </summary>
	public class TextBox : GameObject
	{
		public Point Position;
		public int Lines;
		public float Width;
		public Font Font;
		public int Depth;

		public float BorderWidth = 1f;

		public Color BackGroundColor = Color.Silver;
		public Color BorderColor = Color.Black;
		public Color TextColor = Color.Black;

		private bool HasFocus = false;
		private bool CursorActive;

		public event EventHandler Focused;
		public event EventHandler Defocused;
		public event EventHandler<TextChangedEventArgs> TextChanged;

		public StringBuilder Text = new StringBuilder();
		private string LastTextString = "";

		private string DrawString;

		private float CursorBlinkStep;

		private Rectangle Area;

		public TextBox(Point position, int width, int lines, Font font, int depth = 0)
		{
			Position = position;
			Width = width;
			Lines = lines;
			Font = font;
			Depth = depth;
		}

		/// <summary>
		/// Focuses text input on the textbox if it hasn't focus.
		/// </summary>
		public void Focus()
		{
			//make sure we dont focus multiple times
			if (!HasFocus)
			{
				Input.TextInputReceivers.Add(Text);
				HasFocus = true;

				CursorBlinkStep = 0f;

				if (Focused != null)
					Focused(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Removes focus from the textbox if it has focus.
		/// </summary>
		public void Defocus()
		{
			if (HasFocus)
			{
				Input.TextInputReceivers.Remove(Text);
				HasFocus = false;

				if (Defocused != null)
					Defocused(this, EventArgs.Empty);
			}
		}

		public override void Step()
		{
			Area = new Rectangle(Position, Width, Font.LineHeight * Lines + BorderWidth * 2f);

			if (Input.IsPressed(Button.MouseLeft))
			{
				//focus if on the textbox
				if (Area.IntersectsWith(Input.MousePosition))
					Focus();
				else //defocus if not
					Defocus();
			}

			DrawString = Text.ToString();

			//add caret if focused
			if (HasFocus)
			{
				if (CursorBlinkStep < Game.StepsPerSecond / 2f)
				{
					CursorActive = true;
					DrawString += '|';
				}
				else
					CursorActive = false;

				if (++CursorBlinkStep > Game.StepsPerSecond)
					CursorBlinkStep -= Game.StepsPerSecond;
			}

			//trim
			int charsTrimmed;
			DrawString = Font.WrapString(DrawString, Width - BorderWidth * 2f, Lines, out charsTrimmed);

			if (charsTrimmed > 1)
			{
				if (CursorActive)
					charsTrimmed--;

				Text.Remove(Text.Length - charsTrimmed + 1, charsTrimmed - 1);
			}

			//only throw event when the text was manually changed and the key was accepted
			if (Text.ToString() != LastTextString)
			{
				if (TextChanged != null)
					TextChanged(this, new TextChangedEventArgs(Text.ToString()));

				CursorBlinkStep = 0f;

				LastTextString = Text.ToString();
			}
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(Area, BackGroundColor);
			if (HasFocus)
				DrawX.DrawShapeOutline(Area, BorderColor);
			Font.Draw(DrawString, Position + 1f, TextColor, Alignment.TopLeft, -5);
		}
	}
}
