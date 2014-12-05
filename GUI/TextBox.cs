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

		private bool _hasFocus = false;
		private bool _cursorActive;

		public event EventHandler Focused;
		public event EventHandler Defocused;
		public event EventHandler<TextChangedEventArgs> TextChanged;

		public StringBuilder Text = new StringBuilder();
		private string _lastTextString = "";

		private string _drawString;

		private float _cursorBlinkStep;

		private Rectangle _area;

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
			if (!_hasFocus)
			{
				Input.TextInputReceivers.Add(Text);
				_hasFocus = true;

				_cursorBlinkStep = 0f;

				if (Focused != null)
					Focused(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Removes focus from the textbox if it has focus.
		/// </summary>
		public void Defocus()
		{
			if (_hasFocus)
			{
				Input.TextInputReceivers.Remove(Text);
				_hasFocus = false;

				if (Defocused != null)
					Defocused(this, EventArgs.Empty);
			}
		}

		public override void Step()
		{
			_area = new Rectangle(Position, Width, Font.LineHeight * Lines + BorderWidth * 2f);

			if (Input.IsPressed(Button.MouseLeft))
			{
				//focus if on the textbox
				if (_area.IntersectsWith(Input.MousePosition))
					Focus();
				else //defocus if not
					Defocus();
			}

			_drawString = Text.ToString();

			//add caret if focused
			if (_hasFocus)
			{
				if (_cursorBlinkStep < Game.StepsPerSecond / 2f)
				{
					_cursorActive = true;
					_drawString += '|';
				}
				else
					_cursorActive = false;

				if (++_cursorBlinkStep > Game.StepsPerSecond)
					_cursorBlinkStep -= Game.StepsPerSecond;
			}

			//trim
			int charsTrimmed;
			_drawString = Font.WrapString(_drawString, Width - BorderWidth * 2f, Lines, out charsTrimmed);

			if (charsTrimmed > 1)
			{
				if (_cursorActive)
					charsTrimmed--;

				Text.Remove(Text.Length - charsTrimmed + 1, charsTrimmed - 1);
			}

			//only throw event when the text was manually changed and the key was accepted
			if (Text.ToString() != _lastTextString)
			{
				if (TextChanged != null)
					TextChanged(this, new TextChangedEventArgs(Text.ToString()));

				_cursorBlinkStep = 0f;

				_lastTextString = Text.ToString();
			}
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(_area, BackGroundColor);
			if (_hasFocus)
				DrawX.DrawShapeOutline(_area, BorderColor);
			Font.Draw(_drawString, Position + 1f, TextColor, Alignment.TopLeft, -5);
		}
	}
}
