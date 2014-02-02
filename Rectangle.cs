using System;

namespace HatlessEngine
{
	public struct Rectangle
	{
		public float X;
		public float Y;
		public float Width;
		public float Height;
		public float RelativeOriginX;
		public float RelativeOriginY;
		public float Rotation;

		public float X2
		{
			get { return X + Width; }
			set { Width = value - X; }
		}
		public float Y2
		{
			get { return Y + Height; }
			set { Height = value - Y; }
		}

		public Point Position
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}
		public Point Size
		{
			get { return new Point(Width, Height); }
			set { Width = value.X; Height = value.Y; }
		}

		public Point RelativeOrigin
		{
			get { return new Point(RelativeOriginX, RelativeOriginY); }
			set { RelativeOriginX = value.X; RelativeOriginY = value.Y; }
		}
		public Point AbsoluteOrigin
		{
			get { return new Point(X + RelativeOriginX, Y + RelativeOriginY); }
			set { RelativeOriginX = value.X - X; RelativeOriginY = value.Y - Y; }
		}

		public Point Point1
		{
			get 
			{
				Point point = new Point(X, Y);
				point.RotateOverOrigin(AbsoluteOrigin, Rotation);
				return point;
			}
		}
		public Point Point2
		{
			get 
			{
				Point point = new Point(X2, Y);
				point.RotateOverOrigin(AbsoluteOrigin, Rotation);
				return point;
			}
		}
		public Point Point3
		{
			get 
			{
				Point point = new Point(X2, Y2);
				point.RotateOverOrigin(AbsoluteOrigin, Rotation);
				return point;
			}
		}
		public Point Point4
		{
			get 
			{
				Point point = new Point(X, Y2);
				point.RotateOverOrigin(AbsoluteOrigin, Rotation);
				return point;
			}
		}

		/// <summary>
		/// Returns an array with the 4 points in the rectangle.
		/// Better than directly using Point#.X or Point#.Y (will do the maths twice).
		/// </summary>
		public Point[] Points
		{
			get { return new Point[] { Point1, Point2, Point3, Point4 }; }
		}

		public Rectangle(float x, float y, float width, float height, float relativeOriginX, float relativeOriginY, float rotation = 0)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			RelativeOriginX = relativeOriginX;
			RelativeOriginY = relativeOriginY;
			Rotation = rotation;
		}
		public Rectangle(Point position, Point size, Point relativeOrigin, float rotation = 0)
			: this(position.X, position.Y, size.X, size.Y, relativeOrigin.X, relativeOrigin.Y, rotation) { }
		public Rectangle(float x, float y, float width, float height, float rotation = 0)
			: this(x, y, width, height, width / 2, height / 2, rotation) { }
		public Rectangle(Point position, Point size, float rotation = 0) 
			: this(position.X, position.Y, size.X, size.Y, size.X / 2, size.Y / 2, rotation) { }

		public bool IntersectsWith(Rectangle rectangle)
		{
			//load in all points of the rectangles for efficiency
			Point[] points = {Point1, Point2, Point3, Point4, rectangle.Point1, rectangle.Point2, rectangle.Point3, rectangle.Point4};

			//calculate 2 perpendicular axes for both rectangles (so 4 total) (-Y, X)
			Point[] axes = new Point[4];
			axes[0].X = -points[1].Y + points[0].Y;
			axes[0].Y = points[1].X - points[0].X;
			axes[1].X = -points[2].Y + points[1].Y;
			axes[1].Y = points[2].X - points[1].X;
			axes[2].X = -points[5].Y + points[4].Y;
			axes[2].Y = points[5].X - points[4].X;
			axes[3].X = -points[6].Y + points[5].Y;
			axes[3].Y = points[6].X - points[5].X;

			//project all points onto axes till a non-overlap has been detected by using scalars
			for (byte axis = 0; axis < 4; axis++)
			{
				float thisScalarMin = 0, thisScalarMax = 0, otherScalarMin = 0, otherScalarMax = 0;

				for(byte point = 0; point < 8; point++)
				{
					float multiplier = (float)((points[point].X * axes[axis].X + points[point].Y * axes[axis].Y) / (Math.Pow(axes[axis].X, 2) + Math.Pow(axes[axis].Y, 2)));
					float scalar = multiplier * axes[axis].X * axes[axis].X + multiplier * axes[axis].Y * axes[axis].Y;
				
					if (point == 0)
					{
						thisScalarMin = scalar;
						thisScalarMax = scalar;
					}
					else if (point < 4)
					{
						thisScalarMin = Math.Min(scalar, thisScalarMin);
						thisScalarMax = Math.Max(scalar, thisScalarMax);
					}
					else if (point == 4)
					{
						otherScalarMin = scalar;
						otherScalarMax = scalar;
					}
					else
					{
						otherScalarMin = Math.Min(scalar, otherScalarMin);
						otherScalarMax = Math.Max(scalar, otherScalarMax);
					}
				}

				//one projection does not overlap -> no intersection
				if (thisScalarMax < otherScalarMin || thisScalarMin > otherScalarMax)
					return false;
			}

			//all projections overlapped -> intersection
			return true;
		}
		/// <summary>
		/// Checks against a point (only projects on the 2 axes of this rectangle.
		/// </summary>
		public bool IntersectsWith(Point point)
		{
			Point[] points = { Point1, Point2, Point3, Point4, point };
			Point[] axes = new Point[2];
			axes[0].X = -points[1].Y + points[0].Y;
			axes[0].Y = points[1].X - points[0].X;
			axes[1].X = -points[2].Y + points[1].Y;
			axes[1].Y = points[2].X - points[1].X;

			for(byte axis = 0; axis < 2; axis++)
			{
				float thisScalarMin = 0, thisScalarMax = 0, otherScalar = 0;

				for(byte iPoint = 0; iPoint < 5; iPoint++)
				{
					float multiplier = (float)((points[iPoint].X * axes[axis].X + points[iPoint].Y * axes[axis].Y) / (Math.Pow(axes[axis].X, 2) + Math.Pow(axes[axis].Y, 2)));
					float scalar = multiplier * axes[axis].X * axes[axis].X + multiplier * axes[axis].Y * axes[axis].Y;

					if (iPoint == 0)
					{
						thisScalarMin = scalar;
						thisScalarMax = scalar;
					}
					else if (iPoint < 4)
					{
						thisScalarMin = Math.Min(scalar, thisScalarMin);
						thisScalarMax = Math.Max(scalar, thisScalarMax);
					}
					else
						otherScalar = scalar;
				}

				//one projection does not overlap -> no intersection
				if (thisScalarMax < otherScalar || thisScalarMin > otherScalar)
					return false;
			}

			return true;
		}

		public static explicit operator System.Drawing.RectangleF(Rectangle rect)
		{
			return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		}
		public static explicit operator System.Drawing.Rectangle(Rectangle rect)
		{
			return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}
	}
}

