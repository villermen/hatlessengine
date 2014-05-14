using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Object that has physical properties.
    /// Has built-in support for a sprite, boundbox, collision detection etc.
    /// </summary>
    public class PhysicalObject : LogicalObject
    {
		/// <summary>
		/// Change the position of Bounds easily.
		/// </summary>
		public Point Position
		{
			get { return Bounds.Position; }
			set { Bounds.Position = value; }
		}
		/// <summary>
		/// Builtin speed, will be added to the position after each step while keeping collisionrules in mind.
		/// Can be changed by collision actions.
		/// </summary>
		public Point Speed = Point.Zero;
		private float _SpeedDirection = 0f;
		/// <summary>
		/// Get the direction of the speed or change it while keeping it's velocity.
		/// </summary>
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
		/// <summary>
		/// Get the Velocity (amplitude) of the speed or change it while keeping it's direction.
		/// </summary>
		public float SpeedVelocity
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
		/// For checking whether collisions after the first are still in range.
		/// </summary>
		private float SpeedLeft;

		/// <summary>
		/// The bounds of this object, used for collision detection.
		/// Set to a new shape to update it, or cast it to your type before updating.
		/// </summary>
		public IShape Bounds = Point.Zero;

		public PhysicalObject(Point position) : base()
        {
            //set position
            Position = position;

			//add object to PhysicalObjectsByType along with each basetype up till PhysicalObject
            for (Type currentType = this.GetType(); currentType != typeof(LogicalObject); currentType = currentType.BaseType)
            {
                if (!Resources.PhysicalObjectsByType.ContainsKey(currentType))
                    Resources.PhysicalObjectsByType[currentType] = new List<PhysicalObject>();
				Resources.PhysicalObjectsByType[currentType].Add(this);
			}
        }

		#region Collision handling

        public List<CollisionRule> CollisionRules = new List<CollisionRule>();

        internal override void AfterStep()
        {
			SpeedLeft = 1f;
			do
			{
				float minTouchingSpeedFraction = float.PositiveInfinity;
                Point minTouchingAxis = Point.Zero;
                CollisionRule minTouchingRule = null;
                object minTouchingMethodArg = null;

                CollisionRule ignoreRule = null;

                //decide what object of the rules is closest and within collision range
                foreach(CollisionRule cRule in CollisionRules)
                {
                    //skip if this rule doesn't have to be checked anyway
                    if (cRule == ignoreRule || !cRule.Active)
                        continue;

                    float touchingSpeedFraction = float.PositiveInfinity;
                    Point intersectionAxis;

                    switch (cRule.Type)
                    {
                        case CollisionRuleType.Shape:
                            if (Misc.ShapesIntersectingBySpeed(Bounds, (IShape)cRule.Target, Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                            {
                                minTouchingSpeedFraction = touchingSpeedFraction;
                                minTouchingAxis = intersectionAxis;
                                minTouchingRule = cRule;
                                minTouchingMethodArg = cRule.Target;
                            }
                            break;

                        case CollisionRuleType.Object:
                            if (Misc.ShapesIntersectingBySpeed(Bounds, ((PhysicalObject)cRule.Target).Bounds, Speed - ((PhysicalObject)cRule.Target).Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                            {
                                minTouchingSpeedFraction = touchingSpeedFraction;
                                minTouchingAxis = intersectionAxis;
                                minTouchingRule = cRule;
                                minTouchingMethodArg = cRule.Target;
                            }
                            break;

                        case CollisionRuleType.ObjectType:
                            foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[(Type)cRule.Target])
                            {
                                if (Misc.ShapesIntersectingBySpeed(Bounds, obj.Bounds, Speed - obj.Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                                {
                                    minTouchingSpeedFraction = touchingSpeedFraction;
                                    minTouchingAxis = intersectionAxis;
                                    minTouchingRule = cRule;
                                    minTouchingMethodArg = obj;
                                }  
                            }
                            break;

                        case CollisionRuleType.Objectmap:
                            foreach (PhysicalObject obj in ((Objectmap)cRule.Target).ActivePhysicalObjects)
                            {
                                //whether the object should be checked if the filter is enabled
                                if (!cRule.FilterEnabled || cRule.ObjectmapFilter.Contains(obj.GetType())) //account for inheritance
                                {
                                    if (Misc.ShapesIntersectingBySpeed(Bounds, obj.Bounds, Speed - obj.Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                                    {
                                        minTouchingSpeedFraction = touchingSpeedFraction;
                                        minTouchingAxis = intersectionAxis;
                                        minTouchingRule = cRule;
                                        minTouchingMethodArg = obj;
                                    }
                                }
                            }
                            break;

                        case CollisionRuleType.ManagedSprite:
                            if (Misc.ShapesIntersectingBySpeed(Bounds, ((ManagedSprite)cRule.Target).SpriteRectangle, Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                            {
                                minTouchingSpeedFraction = touchingSpeedFraction;
                                minTouchingAxis = intersectionAxis;
                                minTouchingRule = cRule;
                                minTouchingMethodArg = cRule.Target;
                            }
                            break;

                        case CollisionRuleType.Spritemap:
                            foreach (ManagedSprite sprite in ((Spritemap)cRule.Target).ManagedSprites)
                            {
                                //whether the object should be checked if the filter is enabled
                                if (!cRule.FilterEnabled || cRule.SpritemapFilter.Contains(sprite.TargetSprite)) //account for inheritance
                                {
                                    Rectangle sRect = sprite.SpriteRectangle; //copy over so it the relative position won't be changed
                                    sRect.Position += cRule.SpritemapOffset;

                                    if (Misc.ShapesIntersectingBySpeed(Bounds, sRect, Speed, out touchingSpeedFraction, out intersectionAxis) && touchingSpeedFraction < minTouchingSpeedFraction)
                                    {
                                        minTouchingSpeedFraction = touchingSpeedFraction;
                                        minTouchingAxis = intersectionAxis;
                                        minTouchingRule = cRule;
                                        minTouchingMethodArg = sprite;
                                    }
                                }
                            }
                            break;
                    }
                }

				
				if (minTouchingSpeedFraction < SpeedLeft)
				{
                    //collision is within range, perform it

					//move to touching point
					Position += Speed * minTouchingSpeedFraction;
					SpeedLeft -= minTouchingSpeedFraction;

                    switch (minTouchingRule.Action)
                    {
                        case CollisionAction.Block:
                            Speed = 0f;
						    SpeedLeft = 0f;
                            break;

                        case CollisionAction.Bounce:
                            SpeedDirection = Speed.Angle - 180 + (minTouchingAxis.Angle - Speed.Angle) * 2;
                            break;
                    }

                    minTouchingRule.CallMethod(minTouchingMethodArg);

                    if (minTouchingRule.DeactivateAfterCollision)
                        minTouchingRule.Active = false;

                    ignoreRule = minTouchingRule;
				}
				else 
				{
                    //move the remaining distance
					Position += Speed * SpeedLeft;
					SpeedLeft = 0f;
				}
			} while (SpeedLeft > 0f);
        }
			
		/// <summary>
		/// For a direct check on if this object is overlapping with a shape.
		/// </summary>
		public bool IntersectsWith(IShape shape)
		{
			return Misc.ShapesIntersecting(Bounds, shape);
		}

		/// <summary>
		/// Returns whether this object will intersect with the given shape using speed.
		/// Cannot be trusted as there might be collisionrules preventing it from actually happening, or might change it so that it WILL actually happen.
		/// Great for quick checks though...
		/// </summary>
		public bool IntersectsBySpeed(IShape shape)
		{
			float touchingAtSpeedFraction;
			Point intersectionAxis;
			return Misc.ShapesIntersectingBySpeed(Bounds, shape, Speed, out touchingAtSpeedFraction, out intersectionAxis);
		}

		#endregion

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
