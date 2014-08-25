using System;

namespace HatlessEngine
{
	public class GUIScrollbar : LogicalObject
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

		private float CurrentLengthOffset = 0f;

		private Rectangle ScrollbarRect;
		private Rectangle SliderRect;
		private Rectangle ScrollArea;
		private bool Hovering = false;
		private bool Dragging = false;
		private Point DragStartPos;
		private float DragStartLengthOffset;

		public GUIScrollbar(Point position, float length, float contentLength, float scrollAreaOffset, bool horizontal = false, int depth = 0)
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
			float nextLengthOffset = CurrentLengthOffset;

			if (!Horizontal)
			{
				ScrollbarRect = new Rectangle(Position, Width, Length);
				SliderRect = new Rectangle(Position.X + SliderOffset, Position.Y + SliderOffset + CurrentLengthOffset, Width - SliderOffset * 2f, (float)Math.Pow(Length - SliderOffset, 2) / ContentLength);

				if (ScrollAreaOffset >= 0f)
					ScrollArea = new Rectangle(Position.X, Position.Y, ScrollAreaOffset + Width, Length);
				else
					ScrollArea = new Rectangle(Position.X + ScrollAreaOffset, Position.Y, -ScrollAreaOffset + Width, Length);

				//moving slider by mouse
				if (Dragging)
					nextLengthOffset = Math.Min(Length - SliderOffset - SliderRect.Size.Y, Math.Max(0f, DragStartLengthOffset + Input.UntranslatedMousePosition.Y - DragStartPos.Y));

				//moving slider by mousewheel
				if (ScrollArea.IntersectsWith(Input.MousePosition))
				{
					if (Input.IsPressed(Button.MousewheelUp))
						nextLengthOffset = Math.Max(0f, nextLengthOffset - (Length - SliderOffset * 2f) / 20f);

					if (Input.IsPressed(Button.MousewheelDown))
						nextLengthOffset = Math.Min(Length - SliderOffset - SliderRect.Size.Y, nextLengthOffset + (Length - SliderOffset * 2f) / 20f);
				}				
			}
			else
			{
				ScrollbarRect = new Rectangle(Position, Length, Width);
				SliderRect = new Rectangle(Position.X + SliderOffset + CurrentLengthOffset, Position.Y + SliderOffset, (float)Math.Pow(Length - SliderOffset * 2f, 2) / ContentLength, Width - SliderOffset * 2f);

				if (ScrollAreaOffset >= 0f)
					ScrollArea = new Rectangle(Position.X, Position.Y, Length, ScrollAreaOffset + Width);
				else
					ScrollArea = new Rectangle(Position.X, Position.Y + ScrollAreaOffset, Length, -ScrollAreaOffset + Width);

				//moving slider by mouse
				if (Dragging)
					nextLengthOffset = Math.Min(Length - SliderOffset - SliderRect.Size.X, Math.Max(0f, DragStartLengthOffset + Input.UntranslatedMousePosition.X - DragStartPos.X));

				//moving slider by mousewheel
				if (ScrollArea.IntersectsWith(Input.MousePosition))
				{
					if (Input.IsPressed(Button.MousewheelLeft))
						nextLengthOffset = Math.Max(0f, nextLengthOffset - (Length - SliderOffset * 2f) / 20f);

					if (Input.IsPressed(Button.MousewheelRight))
						nextLengthOffset = Math.Min(Length - SliderOffset - SliderRect.Size.X, nextLengthOffset + (Length - SliderOffset * 2f) / 20f);
				}				
			}

			//things that both directions do the same
			if (SliderRect.IntersectsWith(Input.MousePosition))
			{
				Hovering = true;

				if (Input.IsPressed(Button.MouseLeft))
				{
					Dragging = true;
					DragStartPos = Input.UntranslatedMousePosition;
					DragStartLengthOffset = CurrentLengthOffset;
				}
			}
			else
				Hovering = false;

			if (Input.IsReleased(Button.MouseLeft))
				Dragging = false;

			if (nextLengthOffset != CurrentLengthOffset)
			{
				CurrentLengthOffset = nextLengthOffset;
				if (SliderMoved != null)
					SliderMoved(this, new SliderMovedEventArgs(CurrentLengthOffset / (Length - SliderOffset * 2f) * ContentLength, CurrentLengthOffset / (Length - SliderOffset * 2f)));
			}
		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(ScrollbarRect, BackgroundColor, Depth);
			DrawX.DrawFilledRect(SliderRect, Dragging ? SliderColorDragging : Hovering ? SliderColorHovering : SliderColor, Depth);
		}
	}
}
