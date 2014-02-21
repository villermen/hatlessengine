using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	public interface IShape
	{
		Point Position { get; set; }

		/// <summary>
		/// Returns an array with all the points of the shape.
		/// </summary>
		Point[] Points { get; }
		/// <summary>
		/// Returns an array with relevant normalized perpendicular axes;
		/// </summary>
		Point[] Axes { get; }

		/// <summary>
		/// Check if this shape overlaps another
		/// </summary>
		bool IntersectsWith(IShape shape);
	}
}

