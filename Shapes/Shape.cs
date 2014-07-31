using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents a convex shape that can be SAT-checked against.
	/// </summary>
	public abstract class Shape : IConvexShape
	{
		/// <summary>
		/// The absolute position of this shape, add this to each point.
		/// </summary>
		protected Point _Position = Point.Zero;

		/// <summary>
		/// The size of the shape, multiply the basepoints of this shape from the origin with this.
		/// </summary>
		protected Point _Size = new Point(1f, 1f);

		/// <summary>
		/// The rotation of the shape, rotate each point with this value over the origin.
		/// </summary>
		protected float _Rotation = 0f;

		public Point Position
		{
			get { return _Position; }
			set
			{
				_Position = value;
				Changed = true;
			}
		}
		
		public Point Size
		{
			get { return _Size; }
			set
			{
				_Size = value;
				Changed = true;
			}
		}

		public float Rotation
		{
			get { return _Rotation; }
			set
			{
				_Rotation = value;
				Changed = true;
			}
		}

		/// <summary>
		/// Whether any property (e.g. the position) used to calculate the more complex values (e.g. the points) has changed.
		/// If changed it will call Recalculate() before returning any of these complex values.
		/// This system is here to speed up accessing the complex properties by only having to recalculate once when things have changed.
		/// </summary>
		protected bool Changed = true;

		/// <summary>
		/// Recalculate all the complex values of this shape.
		/// Must at least calculate Points, PerpAxes, EnclosingRectangle and BoundLines.
		/// At the very end set Changed to false.
		/// </summary>
		protected abstract void Recalculate();

		/// <summary>
		/// The actual transformed points of the shape.
		/// </summary>
		protected Point[] Points;

		/// <summary>
		/// The relevant normalized perpendicular axes of the sides of this shape.
		/// If multiple sides have the same axis include the EXACT SAME axis at this side's position. The collision check will skip it this way.
		/// </summary>
		protected Point[] PerpAxes;

		/// <summary>
		/// The smallest possible rectangle enclosing this shape.
		/// Just loop through all the points and find the minimum and maximum of all X's and Y's to calculate this easily.
		/// Useful for things like quick collision checks for drawing and such.
		/// </summary>
		protected Rectangle EnclosingRectangle;

		/// <summary>
		/// Lines for all the sides of this shape.
		/// </summary>
		protected Line[] BoundLines;

		/// <summary>
		/// Returns a list with all the points of the shape.
		/// </summary>
		public Point[] GetPoints()
		{
			if (Changed)
				Recalculate();

			return Points;
		}

		/// <summary>
		/// Returns an array with relevant normalized perpendicular axes, for use in collision checking mainly.
		/// </summary>
		public Point[] GetPerpAxes()
		{
			if (Changed)
				Recalculate();

			return PerpAxes;
		}

		/// <summary>
		/// Gets the smallest SimpleRectangle that this shape could fit in.
		/// So basically it's minimum to maximum on the horizontal and vertical axis.
		/// </summary>
		public Rectangle GetEnclosingRectangle()
		{
			if (Changed)
				Recalculate();

			return EnclosingRectangle;
		}

		/// <summary>
		/// Gets the lines for all the sides of this shape.
		/// </summary>
		public Line[] GetBoundLines()
		{
			if (Changed)
				Recalculate();

			return BoundLines;
		}

		/// <summary>
		/// Returns whether this shape overlaps with another somewhere if it moves by relativeSpeed.
		/// Returns the fraction of the speed given at which they touch and the axis closest to this touching point (from either shape).
		/// The fraction will be -1f if there's an overlap but no collision caused by the relativeSpeed.
		/// </summary>
		public bool IntersectsWith(IConvexShape shape, Point relativeSpeed, out float touchingAtSpeedFraction, out Point intersectionAxis)
		{
			touchingAtSpeedFraction = float.PositiveInfinity;
			intersectionAxis = Point.Zero;

			Type shape1Type = GetType();
			Type shape2Type = shape.GetType();

			//2 AABB's or ComplexRectangles without a rotation (so basically AABB's)
			if ((shape1Type == typeof(Rectangle) 
				|| shape1Type == typeof(ComplexRectangle) && _Rotation == 0f)
				&& (shape2Type == typeof(Rectangle)
				|| shape2Type == typeof(ComplexRectangle) && ((Shape)shape)._Rotation == 0f))
			{
				Rectangle rect1 = (Rectangle)this;
				Rectangle rect2 = (Rectangle)shape;
				float xTouchingFraction, yTouchingFraction;

				if (relativeSpeed.X >= 0f)
				{
					if (rect1.Position2.X + relativeSpeed.X < rect2.Position.X || rect1.Position.X > rect2.Position2.X)
						return false;

					xTouchingFraction = (rect2.Position.X - rect1.Position2.X) / relativeSpeed.X;
				}
				else
				{
					if (rect1.Position.X + relativeSpeed.X > rect2.Position2.X || rect1.Position2.X < rect2.Position.X)
						return false;

					xTouchingFraction = (rect2.Position2.X - rect1.Position.X) / relativeSpeed.X;
				}

				if (relativeSpeed.Y >= 0f)
				{
					if (rect1.Position2.Y + relativeSpeed.Y < rect2.Position.Y || rect1.Position.Y > rect2.Position2.Y)
						return false;

					yTouchingFraction = (rect2.Position.Y - rect1.Position2.Y) / relativeSpeed.Y;
				}
				else
				{
					if (rect1.Position.Y + relativeSpeed.Y > rect2.Position2.Y || rect1.Position2.Y < rect2.Position.Y)
						return false;

					yTouchingFraction = (rect2.Position2.Y - rect1.Position.Y) / relativeSpeed.Y;
				}

				//decide which fraction and corresponding axis to return, if any
				if (xTouchingFraction >= 0f
					&& xTouchingFraction <= 1f)
				{
					touchingAtSpeedFraction = xTouchingFraction;
					intersectionAxis = rect2.GetPerpAxes()[0];
				}

				if (yTouchingFraction >= 0f
					&& yTouchingFraction <= 1f
					&& yTouchingFraction < touchingAtSpeedFraction)
				{
					touchingAtSpeedFraction = yTouchingFraction;
					intersectionAxis = rect2.GetPerpAxes()[1];
				}

				if (intersectionAxis == Point.Zero)
					touchingAtSpeedFraction = -1f;

				return true;
			}
			else //other shapes
			{
				touchingAtSpeedFraction = float.PositiveInfinity;

				Point[] shape1Points = GetPoints();
				Point[] shape2Points = shape.GetPoints();

				List<Point> allAxes = new List<Point>(GetPerpAxes());
				allAxes.AddRange(shape.GetPerpAxes());

				foreach (Point axis in allAxes)
				{
					float shape1ScalarMin = float.PositiveInfinity;
					float shape1ScalarMax = float.NegativeInfinity;
					float shape2ScalarMin = float.PositiveInfinity;
					float shape2ScalarMax = float.NegativeInfinity;

					//cast shape1's points to the axis
					foreach (Point point in shape1Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape1ScalarMin)
							shape1ScalarMin = scalar;
						if (scalar > shape1ScalarMax)
							shape1ScalarMax = scalar;
					}

					//cast shape2's points to the axis
					foreach (Point point in shape2Points)
					{
						float multiplier = (float)((point.X * axis.X + point.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
						float scalar = multiplier * axis.X * axis.X + multiplier * axis.Y * axis.Y;

						if (scalar < shape2ScalarMin)
							shape2ScalarMin = scalar;
						if (scalar > shape2ScalarMax)
							shape2ScalarMax = scalar;
					}

					//cast speed to axis
					float speedMultiplier = (float)((relativeSpeed.X * axis.X + relativeSpeed.Y * axis.Y) / (axis.X * axis.X + axis.Y * axis.Y));
					float speedScalar = speedMultiplier * axis.X * axis.X + speedMultiplier * axis.Y * axis.Y;

					float thisTouchingAtSpeedFraction;
					if (speedScalar >= 0)
					{
						if (shape1ScalarMax + speedScalar < shape2ScalarMin || shape1ScalarMin > shape2ScalarMax)
							return false;

						thisTouchingAtSpeedFraction = (shape2ScalarMin - shape1ScalarMax) / speedScalar;
					}
					else
					{
						if (shape1ScalarMin + speedScalar > shape2ScalarMax || shape1ScalarMax < shape2ScalarMin)
							return false;

						thisTouchingAtSpeedFraction = (shape2ScalarMax - shape1ScalarMin) / speedScalar;
					}

					if (thisTouchingAtSpeedFraction >= 0f
						&& thisTouchingAtSpeedFraction <= 1f
						&& thisTouchingAtSpeedFraction < touchingAtSpeedFraction)
					{
						touchingAtSpeedFraction = thisTouchingAtSpeedFraction;
						intersectionAxis = axis;
					}
				}

				if (intersectionAxis == Point.Zero)
					touchingAtSpeedFraction = -1f;
				else //this axis is still a perpendicular axis, revert!
					intersectionAxis = new Point(intersectionAxis.Y, -intersectionAxis.X);

				return true;
			}
		}
		/// <summary>
		/// Returns whether this shape overlaps with another somewhere if it moves by relativeSpeed.
		/// </summary>
		public bool IntersectsWith(IConvexShape shape, Point relativeSpeed)
		{
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			return IntersectsWith(shape, relativeSpeed, out touchingAtSpeedFraction, out intersectionAxis);
		}
		/// <summary>
		/// Returns whether this shape overlaps with another.
		/// </summary>
		public bool IntersectsWith(IConvexShape shape)
		{
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			return IntersectsWith(shape, Point.Zero, out touchingAtSpeedFraction, out intersectionAxis);
		}

		public override string ToString()
		{
			if (Changed)
				Recalculate();

			string str = "";
			foreach (Point point in Points)
				str += point.ToString();

			return str;
		}
	}
}