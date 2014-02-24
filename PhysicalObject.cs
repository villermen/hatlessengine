using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Object that has a physical position and size in the game.
    /// Has built-in support for a sprite, boundbox etc.
    /// </summary>
    public class PhysicalObject : LogicalObject
    {
		public enum CollisionAction
		{
			/// <summary>
			/// Do not take action.
			/// </summary>
			None = 0,
			/// <summary>
			/// Block this object at touching setting speed to 0.
			/// </summary>
			Block = 1,
			[Obsolete("Not implemented yet.", true)]
			/// <summary>
			/// Block both objects at touching point setting speed to 0.
			/// </summary>
			BlockBoth = 2,
			/// <summary>
			/// Bounce perfectly off the surface changing the SpeedDirection accordingly.
			/// </summary>
			Bounce = 3,
			[Obsolete("Not implemented yet.", true)]
			BounceBoth = 4,
			[Obsolete("Not implemented yet.", true)]
			Slide = 5
		}

		public Point Position
		{
			get { return BoundBox.Position; }
			set { BoundBox.Position = value; }
		}
		private Point PreviousPosition = Point.Zero;
		/// <summary>
		/// Builtin speed, will be added to the position after each step.
		/// Will be set after collision actions, does not get read for them.
		/// </summary>
		public Point Speed = Point.Zero;
		private float _SpeedDirection = 0f;
		public float SpeedDirection
		{
			get 
			{
				//get the right direction when it cannot be calculated from the vector components
				if (Speed.X == 0f && Speed.Y == 0f)
					return _SpeedDirection;
				return Speed.Angle;
			}
			set 
			{ 
				float length = Speed.Length;
				Speed.X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * length;
				Speed.Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * length;
				_SpeedDirection = value;
			}
		}
		public float SpeedAmplitude
		{
			get 
			{
				return Speed.Length;
			}
			set 
			{ 
				float direction = Speed.Angle;
				Speed.X = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * value;
				Speed.Y = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * value;
			}
		}
		/// <summary>
		/// Reduced by collision actions.
		/// </summary>
		private float SpeedLeft = 1f;

		public Rectangle BoundBox = Rectangle.Zero;

		public PhysicalObject(Point position) : base()
        {
            //set position
            Position = position;
			BoundBox.Position = position;

            //add object to PhysicalObjectsByType along with each basetype up till PhysicalObject
            for (Type currentType = this.GetType(); currentType != typeof(LogicalObject); currentType = currentType.BaseType)
            {
                if (!Resources.PhysicalObjectsByType.ContainsKey(currentType))
                    Resources.PhysicalObjectsByType[currentType] = new List<PhysicalObject>();
                Resources.PhysicalObjectsByType[currentType].Add(this);
            }
        }

        internal override void AfterStep()
        {
			//move however much there is left to move
			Position += Speed * SpeedLeft;
			SpeedLeft = 1f;
        }

		/// <summary>
		/// Collision with a moving shape.
		/// </summary>
		public bool Collision(IShape shape, Point shapeSpeed, CollisionAction action = CollisionAction.None)
        {
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			Point relativeSpeed = Speed - shapeSpeed;
			//if we can still reach the touching point
			if (Misc.ShapesIntersectingWithSpeed(BoundBox, shape, relativeSpeed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction <= SpeedLeft)
			{
				//-1 to prevent most of the 'Hi I'm high speed and I don't care' passthroughs
				if (touchingAtSpeedFraction >= 0f)
				{
					if (action == CollisionAction.Block)
					{
						Position += Speed * touchingAtSpeedFraction;
						Speed = 0f;
						SpeedLeft = 0f;
					}
					if (action == CollisionAction.Bounce)
					{
						Position += Speed * touchingAtSpeedFraction;
						SpeedDirection = relativeSpeed.Angle - 180 + (intersectionAxis.Angle - relativeSpeed.Angle) * 2;
						SpeedLeft -= touchingAtSpeedFraction;
					}
				}
				return true;
			}
			return false;
        }
		/// <summary>
		/// Collision with a static shape
		/// </summary>
		public bool Collision(IShape shape, CollisionAction action = CollisionAction.None)
		{
			return Collision(shape, Point.Zero, action);
		}
		/// <summary>
		/// Collision with an object.
		/// </summary>
		public bool Collision(PhysicalObject obj, CollisionAction action = CollisionAction.None)
		{
			return Collision(obj.BoundBox, obj.Speed, action);
		}
		/// <summary>
		/// Collision with any object of the given type.
		/// Will perform all collisions if action is not CollisionAction.None to make sure everything is accounted for.
		/// Will stop at the first collision if only a bool is needed.
		/// </summary>
		public bool Collision(Type objType, CollisionAction action = CollisionAction.None)
		{
			bool result = false;
			bool tempResult;
			foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[objType])
			{
				tempResult = Collision(obj.BoundBox, obj.Speed, action);
				if (tempResult && action != CollisionAction.None)
					result = true;
				if (tempResult && action == CollisionAction.None)
					return true;
			}
			return result;
		}
		/// <summary>
		/// Collision with a ManagedSprite.
		/// </summary>
		public bool Collision(ManagedSprite mSprite, CollisionAction action = CollisionAction.None)
		{
			return Collision(mSprite.SpriteRectangle, Point.Zero, action);
		}
		/// <summary>
		/// Collision with all ManagedSprites in a Spritemap, or with all ManagedSprites with the given name in the Spritemap.
		/// Will perform all collisions if action is not CollisionAction.None to make sure everything is accounted for.
		/// Will stop at the first collision if only a bool is needed.
		/// </summary>
		public bool Collision(string spritemapId, Point spritemapPos, string spriteNameFilter = "", CollisionAction action = CollisionAction.None)
		{
			bool result = false;
			bool tempResult;
			Spritemap sMap = Resources.Spritemaps[spritemapId];
			foreach(ManagedSprite mSprite in sMap.ManagedSprites)
			{
				if (spriteNameFilter == "" || mSprite.TargetSprite.Id == spriteNameFilter)
				{
					Rectangle spriteRect = mSprite.SpriteRectangle; //clone, don't use the actual mSprite
					spriteRect.Position += spritemapPos;

					tempResult = Collision(spriteRect, Point.Zero, action);
					if (tempResult && action != CollisionAction.None)
						result = true;
					if (tempResult && action == CollisionAction.None)
						return true;
				}
			}
			return result;
		}

        public new void Destroy()
        {
            base.Destroy();

            //remove from physicalobjectsbytype
            for (Type currentType = this.GetType(); currentType != typeof(LogicalObject); currentType = currentType.BaseType)
            {
                Resources.PhysicalObjectsByType[currentType].Remove(this);
            }
        }
    }
}
