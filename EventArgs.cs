using System;

namespace HatlessEngine
{
	public class SliderMovedEventArgs : EventArgs
	{
		/// <summary>
		/// Offset of the target content area.
		/// Apply the negative of this value to the content you wish to scroll.
		/// </summary>
		public float ContentOffset;
		/// <summary>
		/// Offset fraction off the target content area, based on it's total length.
		/// Also happens to be the offset fraction on the slider, convenient right?
		/// </summary>
		public float ContentOffsetFraction;

		public SliderMovedEventArgs(float contentOffset, float contentOffsetFraction)
		{
			ContentOffset = contentOffset;
			ContentOffsetFraction = contentOffsetFraction;
		}
	}

	public class TextChangedEventArgs : EventArgs
	{
		public string NewText;

		public TextChangedEventArgs(string newText)
		{
			NewText = newText;
		}
	}
}