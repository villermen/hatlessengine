using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a generic convex shape interface that can be SAT-checked against.
	/// </summary>
	public interface IShape
	{
		Point Position { get; set; }
		float Rotation { get; set; }

		/// <summary>
		/// Returns a list with all the points of the shape.
		/// </summary>
		Point[] GetPoints();

		/// <summary>
		/// Returns an array with relevant normalized perpendicular axes, for use in the Separating Axis Theorem.
		/// </summary>
		Point[] GetPerpAxes();

		/// <summary>
		/// Gets the smallest SimpleRectangle that this shape could fit in.
		/// So basically it's minimum to maximum on the horizontal and vertical axis.
		/// </summary>
		Rectangle GetEnclosingRectangle();
	}
}