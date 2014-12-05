using System;

namespace HatlessEngine.GUI
{
	public class VerticalScrollbar : GameObject
	{
		public bool Horizontal { get; private set; }

		/// <summary>
		/// Length of the scrollbar.
		/// Height if vertical, width if horizontal.
		/// Should be the same as the length of the (visible part of the) content pane in the relevant direction.
		/// </summary>
		public float Length;

		/// <summary>
		/// Total length of the content being scrolled through.
		/// Height if vertical, width if horizontal.
		/// </summary>
		public float ContentLength;

		/// <summary>
		/// If the mouse is this far negative or positive of the scrollbar the scrollwheel can be used to move the slider.
		/// </summary>
		public float ScrollAreaOffset;

		/// <summary>
		/// Top-left corner of the scrollbar.
		/// </summary>
		public Point Position;

		public int Depth;

		/// <summary>
		/// The width of the scrollbar.
		/// Height if horizontal, width if vertical. (yes this is confusing, shut up)
		/// </summary>
		public float Width = 14f;

		/// <summary>
		/// The distance between both sides and the actual slider.
		/// </summary>
		public float SliderOffset = 2f;

		public Color BackgroundColor = new Color(60, 60, 60);
		public Color SliderColor = new Color(130, 130, 130);
		public Color SliderColorHovering = new Color(180, 180, 180);
		public Color SliderColorDragging = new Color(230, 230, 230);

		/// <summary>
		/// Occurs whenever the slider moves (so once every step it's being dragged and it's position has changed).
		/// </summary>
		public event EventHandler<SliderMovedEventArgs> SliderMoved;

		private float _currentLengthOffset = 0f;

		private Rectangle _scrollbarRect;
		private Rectangle _sliderRect;
		private Rectangle _scrollArea;
		private bool _hovering = false;
		private bool _dragging = false;
		private Point _dragStartPos;
		private float _dragStartLengthOffset;

		public VerticalScrollbar(Point position, float length, float contentLength, float scrollAreaOffset, bool horizontal = false, int depth = 0)
		{
			Horizontal = horizontal;
			Position = position;
			Length = length;
			ContentLength = contentLength;
			ScrollAreaOffset = scrollAreaOffset;
			Depth = depth;
		}

		public override void Step()
		{
			float nextLengthOffset = _currentLengthOffset;

			if (!Horizontal)
			{
				_scrollbarRect = new Rectangle(Position, Width, Length);
				_sliderRect = new Rectangle(Position.X + SliderOffset, Position.Y + SliderOffset + _currentLengthOffset, Width - SliderOffset * 2f, (float)Math.Pow(Length - SliderOffset, 2) / ContentLength);

				if (ScrollAreaOffset >= 0f)
					_scrollArea = new Rectangle(Position.X, Position.Y, ScrollAreaOffset + Width, Length);
				else
					_scrollArea = new Rectangle(Position.X + ScrollAreaOffset, Position.Y, -ScrollAreaOffset + Width, Length);

				//moving slider by mouse
				if (_dragging)
					nextLengthOffset = Math.Min(Length - SliderOffset - _sliderRect.Size.Y, Math.Max(0f, _dragStartLengthOffset + Input.UntranslatedMousePosition.Y - _dragStartPos.Y));

				//moving slider by mousewheel
				if (_scrollArea.IntersectsWith(Input.MousePosition))
				{
					if (Input.IsPressed(Button.MousewheelUp))
						nextLengthOffset = Math.Max(0f, nextLengthOffset - (Length - SliderOffset * 2f) / 20f);

					if (Input.IsPressed(Button.MousewheelDown))
						nextLengthOffset = Math.Min(Length - SliderOffset - _sliderRect.Size.Y, nextLengthOffset + (Length - SliderOffset * 2f) / 20f);
				}				
			}
			else
			{
				_scrollbarRect = new Rectangle(Position, Length, Width);
				_sliderRect = new Rectangle(Position.X + SliderOffset + _currentLengthOffset, Position.Y + SliderOffset, (float)Math.Pow(Length - SliderOffset * 2f, 2) / ContentLength, Width - SliderOffset * 2f);

				if (ScrollAreaOffset >= 0f)
					_scrollArea = new Rectangle(Position.X, Position.Y, Length, ScrollAreaOffset + Width);
				else
					_scrollArea = new Rectangle(Position.X, Position.Y + ScrollAreaOffset, Length, -ScrollAreaOffset + Width);

				//moving slider by mouse
				if (_dragging)
					nextLengthOffset = Math.Min(Length - SliderOffset - _sliderRect.Size.X, Math.Max(0f, _dragStartLengthOffset + Input.UntranslatedMousePosition.X - _dragStartPos.X));

				//moving slider by mousewheel
				if (_scrollArea.IntersectsWith(Input.MousePosition))
				{
					if (Input.IsPressed(Button.MousewheelLeft))
						nextLengthOffset = Math.Max(0f, nextLengthOffset - (Length - SliderOffset * 2f) / 20f);

					if (Input.IsPressed(Button.MousewheelRight))
						nextLengthOffset = Math.Min(Length - SliderOffset - _sliderRect.Size.X, nextLengthOffset + (Length - SliderOffset * 2f) / 20f);
				}				
			}

			//things that both directions do the same
			if (_sliderRect.IntersectsWith(Input.MousePosition))
			{
				_hovering = true;

				if (Input.IsPressed(Button.MouseLeft))
				{
					_dragging = true;
					_dragStartPos = Input.UntranslatedMousePosition;
					_dragStartLengthOffset = _currentLengthOffset;
				}
			}
			else
				_hovering = false;

			if (Input.IsReleased(Button.MouseLeft))
				_dragging = false;

			if (nextLengthOffset == _currentLengthOffset) 
				return;

			_currentLengthOffset = nextLengthOffset;
			if (SliderMoved != null)
				SliderMoved(this, new SliderMovedEventArgs(_currentLengthOffset / (Length - SliderOffset * 2f) * ContentLength, _currentLengthOffset / (Length - SliderOffset * 2f)));
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(_scrollbarRect, BackgroundColor, Depth);
			DrawX.DrawFilledRect(_sliderRect, _dragging ? SliderColorDragging : _hovering ? SliderColorHovering : SliderColor, Depth);
		}
	}
}
