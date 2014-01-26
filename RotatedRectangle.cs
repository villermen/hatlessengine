using System;
using System.Drawing;

namespace HatlessEngine
{
	public struct RotatedRectangle
	{
		public float X;
		public float Y;
		public float Width;
		public float Height;

		public PointF Position
		{
			get { return new PointF(X, Y); }
			set { X = value.X; Y = value.Y; }
		}
		public SizeF Size
		{
			get { return new SizeF(Width, Height); }
			set { Width = value.Width; Height = value.Height; }
		}

		public PointF AbsoluteOrigin
		{
			get { return new PointF(X + RelativeOrigin.X, Y + RelativeOrigin.Y); }
			set { RelativeOrigin = new PointF(value.X - X, value.Y - Y); }
		}
		public PointF RelativeOrigin;
		public float Rotation;

		public PointF Point1
		{
			get 
			{
				return Misc.RotatePointOverOrigin(new PointF(X, Y), AbsoluteOrigin, Rotation);
			}
		}
		public PointF Point2
		{
			get 
			{
				return Misc.RotatePointOverOrigin(new PointF(X + Width, Y), AbsoluteOrigin, Rotation);
			}
		}
		public PointF Point3
		{
			get 
			{
				return Misc.RotatePointOverOrigin(new PointF(X + Width, Y + Height), AbsoluteOrigin, Rotation);
			}
		}
		public PointF Point4
		{
			get 
			{
				return Misc.RotatePointOverOrigin(new PointF(X, Y + Height), AbsoluteOrigin, Rotation);
			}
		}

		public RotatedRectangle(float x, float y, float width, float height, PointF relativeOrigin, float rotation = 0)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			RelativeOrigin = relativeOrigin;
			Rotation = rotation;
		}
		public RotatedRectangle(PointF position, SizeF size, PointF relativeOrigin, float rotation = 0)
			: this(position.X, position.Y, size.Width, size.Height, relativeOrigin, rotation) { }
		public RotatedRectangle(RectangleF rectangle, PointF relativeOrigin, float rotation = 0)
			: this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, relativeOrigin, rotation) { }

		public bool IntersectsWith(RotatedRectangle rectangle)
		{
			//load in all points of the rectangles for efficiency
			PointF[] points = {Point1, Point2, Point3, Point4, rectangle.Point1, rectangle.Point2, rectangle.Point3, rectangle.Point4};

			//calculate 2 perpendicular axes for both rectangles (so 4 total) (-Y, X)
			PointF[] axes = new PointF[4];
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
	}
}

