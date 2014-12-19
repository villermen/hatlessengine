using System;

namespace HatlessEngine
{
	[Flags]
	public enum HorizontalAlignment
	{
		Left = 1,
		Center = 2,
		Right = 4,
	}

	[Flags]
	public enum VerticalAlignment
	{
		Top = 8,
		Middle = 16,
		Bottom = 32,
	}

	/// <summary>
	/// Generic enum for aligning anything on both axes.
	/// This enumeration can be flag checked against HorizontalAligment and VerticalAlignment to check for individual positioning.
	/// </summary>
	[Flags]
	public enum CombinedAlignment
	{
		TopLeft = VerticalAlignment.Top | HorizontalAlignment.Left,
		TopCenter = VerticalAlignment.Top | HorizontalAlignment.Center,
		TopRight = VerticalAlignment.Top | HorizontalAlignment.Right,

		MiddleLeft = VerticalAlignment.Middle | HorizontalAlignment.Left,
		MiddleCenter = VerticalAlignment.Middle | HorizontalAlignment.Center,
		MiddleRight = VerticalAlignment.Middle | HorizontalAlignment.Right,

		BottomLeft = VerticalAlignment.Bottom | HorizontalAlignment.Left,
		BottomCenter = VerticalAlignment.Bottom | HorizontalAlignment.Center,
		BottomRight = VerticalAlignment.Bottom | HorizontalAlignment.Right,


		/*Left Top Right Bottom
		TopLeft TopRight BottomLeft BottomRight
		 * if it has no top or bottom it is center, middle?
		 * back to combinations only, but how do we check them (an internal lookup enum, or something more clever?)
		 * */
		
	}
}
