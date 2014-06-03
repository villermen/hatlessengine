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
				Speed.X = (float)Math.Cos((_SpeedDirection / 180 - 0.5) * Math.PI) * value;
				Speed.Y = (float)Math.Sin((_SpeedDirection / 180 - 0.5) * Math.PI) * value;
			}
		}

		/// <summary>
		/// The bounds of this object, used for collision detection.
		/// Set to a new shape to update it, or cast it to your type before updating.
		/// </summary>
		public IShape Bounds = Point.Zero;

		public PhysicalObject(Point position) : base()
        {
            //set position
            Position = position;

            Resources.PhysicalObjects.Add(this);

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

        /// <summary>
        /// The area the bounds can maximally cover given the speed's velocity.
        /// Per-step, updated after step and before collision handling.
        /// </summary>
        internal Rectangle CoverableArea;

        /// <summary>
        /// For checking whether collisions are still in range, and whether they should still have to be checked.
        /// </summary>
        internal float SpeedLeft;

        internal float ClosestCollisionSpeedFraction = float.PositiveInfinity;
        private Point ClosestCollisionTouchingAxis;
        private CollisionRule ClosestCollisionRule;
        private object ClosestCollisionMethodArg;
        private CollisionRule IgnoreRule = null;

        /// <summary>
        /// Sets the area the bounds could maximally cover with the current SpeedVelocity (any direction).
        /// Rectangle on x and y axes.
        /// </summary>
        internal void UpdateCoverableArea()
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            float speed = SpeedVelocity;

            foreach (Point pnt in Bounds.Points)
            {
                if (pnt.X < minX)
                    minX = pnt.X;
                if (pnt.X > maxX)
                    maxX = pnt.X;
                if (pnt.Y < minY)
                    minY = pnt.Y;
                if (pnt.Y > maxY)
                    maxY = pnt.Y;
            }

            CoverableArea = new Rectangle(minX - speed, minY - speed, maxX - minX + speed, maxY - minY + speed);
        }

        /// <summary>
        /// Calculates the first occuring collision event on this object's track using the collisionrules.
        /// </summary>
        internal float CalculateClosestCollision(List<PhysicalObject> possibleObjectTargets)
        {
            if (SpeedLeft == 0f)
                return -1f;

            float touchingSpeedLeftFraction = float.PositiveInfinity;
            Point intersectionAxis;

            ClosestCollisionSpeedFraction = float.PositiveInfinity;

            //decide which target of the rules is closest and within collision range
            foreach(CollisionRule cRule in CollisionRules)
            {
                //skip if this rule doesn't have to be checked anyway
                if (cRule == IgnoreRule || !cRule.Active)
                    continue;

                switch (cRule.Type)
                {
                    case CollisionRuleType.Shape:
                        if (Misc.ShapesIntersectingBySpeed(Bounds, (IShape)cRule.Target, Speed * SpeedLeft, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                        {
                            ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
                            ClosestCollisionTouchingAxis = intersectionAxis;
                            ClosestCollisionRule = cRule;
                            ClosestCollisionMethodArg = cRule.Target;
                        }
                        break;

                    case CollisionRuleType.Object:
                        if (possibleObjectTargets.Contains((PhysicalObject)cRule.Target) && Misc.ShapesIntersectingBySpeed(Bounds, ((PhysicalObject)cRule.Target).Bounds, (Speed - ((PhysicalObject)cRule.Target).Speed) * SpeedLeft, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction <= ((PhysicalObject)cRule.Target).SpeedLeft && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                        {
                            ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
                            ClosestCollisionTouchingAxis = intersectionAxis;
                            ClosestCollisionRule = cRule;
                            ClosestCollisionMethodArg = cRule.Target;
                        }
                        break;

                    case CollisionRuleType.ObjectType:
                        foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[(Type)cRule.Target])
                        {
                            if (possibleObjectTargets.Contains(obj) && Misc.ShapesIntersectingBySpeed(Bounds, obj.Bounds, (Speed - obj.Speed) * SpeedLeft, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction <= obj.SpeedLeft && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                            {
                                ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
                                ClosestCollisionTouchingAxis = intersectionAxis;
                                ClosestCollisionRule = cRule;
                                ClosestCollisionMethodArg = obj;
                            }  
                        }
                        break;

                    case CollisionRuleType.Objectmap:
                        foreach (PhysicalObject obj in ((Objectmap)cRule.Target).ActivePhysicalObjects)
                        {
                            //whether the object should be checked if the filter is enabled
                            if (!cRule.FilterEnabled || cRule.ObjectmapFilter.Contains(obj.GetType())) //account for inheritance
                            {
                                if (possibleObjectTargets.Contains(obj) && Misc.ShapesIntersectingBySpeed(Bounds, obj.Bounds, (Speed - obj.Speed) * SpeedLeft, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction <= obj.SpeedLeft && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                                {
                                    ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
                                    ClosestCollisionTouchingAxis = intersectionAxis;
                                    ClosestCollisionRule = cRule;
                                    ClosestCollisionMethodArg = obj;
                                }
                            }
                        }
                        break;

                    case CollisionRuleType.ManagedSprite:
                        if (Misc.ShapesIntersectingBySpeed(Bounds, ((ManagedSprite)cRule.Target).SpriteRectangle, Speed, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                        {
                            ClosestCollisionSpeedFraction = touchingSpeedLeftFraction;
                            ClosestCollisionTouchingAxis = intersectionAxis;
                            ClosestCollisionRule = cRule;
                            ClosestCollisionMethodArg = cRule.Target;
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

                                if (Misc.ShapesIntersectingBySpeed(Bounds, sRect, Speed, out touchingSpeedLeftFraction, out intersectionAxis) && touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
                                {
                                    ClosestCollisionSpeedFraction = touchingSpeedLeftFraction;
                                    ClosestCollisionTouchingAxis = intersectionAxis;
                                    ClosestCollisionRule = cRule;
                                    ClosestCollisionMethodArg = sprite;
                                }
                            }
                        }
                        break;
                }
            }

            return ClosestCollisionSpeedFraction;
        }

        /// <summary>
        /// Performs the calculated collision.
        /// pair determines whether both object's involved (if they are) will be performed, second one will be called with false to prevent loopdeeloops.
        /// </summary>
        internal void PerformClosestCollision(bool pair = true)
        {
            //if no collision was found move the remaining distance
			if (ClosestCollisionSpeedFraction == float.PositiveInfinity)
            {
                Position += Speed * SpeedLeft;
                SpeedLeft = 0f;
                return;
            }

            //move to touching point
			Position += Speed * ClosestCollisionSpeedFraction;
			SpeedLeft -= ClosestCollisionSpeedFraction;

            switch (ClosestCollisionRule.Action)
            {
                case CollisionAction.Block:
                    Speed = 0f;
					SpeedLeft = 0f;
                    break;

                case CollisionAction.Bounce:
                    SpeedDirection = Speed.Angle - 180 + (ClosestCollisionTouchingAxis.Angle - Speed.Angle) * 2;
                    break;
            }

            ClosestCollisionRule.CallMethod(ClosestCollisionMethodArg);

            if (ClosestCollisionRule.DeactivateAfterCollision)
                ClosestCollisionRule.Active = false;

            IgnoreRule = ClosestCollisionRule;

            //make sure the other object (if any) performs his collision too before it gets rechecked
            if (pair && (ClosestCollisionRule.Type == CollisionRuleType.Object || ClosestCollisionRule.Type == CollisionRuleType.Objectmap || ClosestCollisionRule.Type == CollisionRuleType.ObjectType))
            {
                ((PhysicalObject)ClosestCollisionMethodArg).PerformClosestCollision(false);
            }
        }

		#endregion

        public new void Destroy()
        {
            base.Destroy();

            Resources.PhysicalObjects.Remove(this);

            //remove from physicalobjectsbytype
            for (Type currentType = this.GetType(); currentType != typeof(LogicalObject); currentType = currentType.BaseType)
            {
                Resources.PhysicalObjectsByType[currentType].Remove(this);
            }
        }
    }
}
