using System;

namespace HatlessEngine
{
	public class GUIScrollbar : LogicalObject
	{
		public int Depth;

		/// <summary>
		/// Length of the scrollbar.
		/// Height if vertical, width if horizontal.
		/// Should be the same as the length of the content pane in the relevant direction.
		/// </summary>
		public float Length;

		/// <summary>
		/// Total length of the content being scrolled through.
		/// Height if vertical, width if horizontal.
		/// </summary>
		public float ContentLength;

		/// <summary>
		/// If the mouse is in this area you can scroll to move the slider.
		/// </summary>
		public Rectangle ScrollArea;

		/// <summary>
		/// Top-left corner of the scrollbar.
		/// </summary>
		public Point Position;

		/// <summary>
		/// Code to run whenever the scrollbar moves.
		/// The value represents the offset of the ContentLength from 0 to ContentLength - Length.
		/// Subract this from the startposition of your whatever and it should be sliding juuust fine.
		/// </summary>
		public Action<float> OnChange;

		private float CurrentLengthOffset = 0f;

		private Rectangle SliderRect;
		private bool Dragging = false;
		private Point DragStartPos;
		private float DragStartLengthOffset;

		public GUIScrollbar(Point position, float length, float contentLength, Rectangle scrollArea, Action<float> onChange, int depth = 0)
		{
			Position = position;
			Length = length;
			ContentLength = contentLength;
			ScrollArea = scrollArea;
			OnChange = onChange;
			Depth = depth;
		}

		public override void Step()
		{
			SliderRect = new Rectangle(new Point(Position.X + 1f, Position.Y + 1f + CurrentLengthOffset), new Point(8f, (float)Math.Pow(Length - 2f, 2) / ContentLength));

			float nextLengthOffset = -1f;
			
			if (Input.IsPressed(Button.MouseLeft) && SliderRect.IntersectsWith(Input.MousePosition))
			{
				Dragging = true;
				DragStartPos = Input.MousePosition;
				DragStartLengthOffset = CurrentLengthOffset;
			}

			if (Dragging)
			{
				//move slider
				nextLengthOffset = Math.Min(Length - 1f - SliderRect.Size.Y, Math.Max(0f, DragStartLengthOffset + Input.MousePosition.Y - DragStartPos.Y));

				if (Input.IsReleased(Button.MouseLeft))
					Dragging = false;
			}

			if (Input.IsPressed(Button.MousewheelUp) && (ScrollArea.IntersectsWith(Input.MousePosition) || new Rectangle(Position, new Point(10f, Length)).IntersectsWith(Input.MousePosition)))
					nextLengthOffset = Math.Max(0f, CurrentLengthOffset - (Length - 2f) / 20f);

			if (Input.IsPressed(Button.MousewheelDown) && (ScrollArea.IntersectsWith(Input.MousePosition) || new Rectangle(Position, new Point(10f, Length)).IntersectsWith(Input.MousePosition)))
					nextLengthOffset = Math.Min(Length - 1f - SliderRect.Size.Y, CurrentLengthOffset + (Length - 2f) / 20f);

			if (nextLengthOffset != -1f && nextLengthOffset != CurrentLengthOffset)
			{
				CurrentLengthOffset = nextLengthOffset;
				OnChange.Invoke(CurrentLengthOffset * ContentLength / (Length - 2f));
			}

		}

		public override void Draw()
		{
			DrawX.DrawFilledRect(new Rectangle(Position, new Point(10f, Length)), Color.Black, Depth);
			DrawX.DrawFilledRect(SliderRect, Color.Silver, Depth);
		}
	}
}
