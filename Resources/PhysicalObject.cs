using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Represents an object that has physical properties.
	/// Has built-in support for a position, boundbox, collision and other physical mumbo-jumbo.
	/// </summary>
	public abstract class PhysicalObject : GameObject
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
		private float _speedDirection = 0f;
		/// <summary>
		/// Get the direction of the speed or change it while keeping it's velocity.
		/// </summary>
		public float SpeedDirection
		{
			get 
			{
				//get the right direction when it cannot be calculated from the vector components
				if (Speed.X == 0f && Speed.Y == 0f)
					return _speedDirection;
				return Speed.GetAngleFromOrigin();
			}
			set 
			{ 
				float length = Speed.GetDistanceFromOrigin();
				Speed.X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * length;
				Speed.Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * length;
				_speedDirection = value;
			}
		}
		/// <summary>
		/// Get the Velocity (amplitude) of the speed or change it while keeping it's direction.
		/// </summary>
		public float SpeedVelocity
		{
			get 
			{
				return Speed.GetDistanceFromOrigin();
			}
			set 
			{ 
				Speed.X = (float)Math.Cos((_speedDirection / 180 - 0.5) * Math.PI) * value;
				Speed.Y = (float)Math.Sin((_speedDirection / 180 - 0.5) * Math.PI) * value;
			}
		}

		/// <summary>
		/// The bounds of this object, used for collision detection.
		/// Set to a new shape to update it, or cast it to your type before updating.
		/// </summary>
		public Shape Bounds = Rectangle.Zero;

		protected PhysicalObject() 
		{
			//add object to the physicalobjects list
			Resources.PhysicalObjects.Add(this);

			//add object to PhysicalObjectsByType along with each basetype up till PhysicalObject
			for (Type currentType = GetType(); currentType != typeof(GameObject); currentType = currentType.BaseType)
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
		/// All the PhysicalObjects this one could possibly collide with this step.
		/// </summary>
		internal List<PhysicalObject> CollisionCandidates;

		/// <summary>
		/// For checking whether collisions are still in range, and whether they should still have to be checked.
		/// </summary>
		internal float SpeedLeft;

		internal float ClosestCollisionSpeedFraction = float.PositiveInfinity;
		private Point _closestCollisionTouchingAxis;
		private CollisionRule _closestCollisionRule;
		private object _closestCollisionMethodArg;
		private Point _closestCollisionRelativeSpeed;

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

			foreach (Point pnt in Bounds.GetPoints())
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
		internal void CalculateClosestCollision()
		{
			ClosestCollisionSpeedFraction = float.PositiveInfinity;

			//no (more) collision checking this step please
			if (SpeedLeft == 0f || Speed == Point.Zero)
			{
				SpeedLeft = 0f;
				return;
			}

			//decide which target of the rules is closest and within collision range
			foreach(CollisionRule cRule in CollisionRules)
			{
				//skip if this rule doesn't have to be checked anyway
				if (!cRule.Active)
					continue;

				//get collision candidates once if they will be checked (JIT)
				if (CollisionCandidates == null && (cRule.Type == CollisionRuleType.Object || cRule.Type == CollisionRuleType.ObjectType || cRule.Type == CollisionRuleType.Objectmap))
					CollisionCandidates = Game.QuadTree.GetCollisionCandidates(this);

				float touchingSpeedLeftFraction;
				Point intersectionAxis;
				switch (cRule.Type)
				{
					case CollisionRuleType.Shape:
						if (Bounds.IntersectsWith((Shape)cRule.Target, Speed, out touchingSpeedLeftFraction, out intersectionAxis)
							&& touchingSpeedLeftFraction > 0f
							&& touchingSpeedLeftFraction <= SpeedLeft
							&& touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
						{
							ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
							_closestCollisionTouchingAxis = intersectionAxis;
							_closestCollisionRule = cRule;
							_closestCollisionMethodArg = cRule.Target;
							_closestCollisionRelativeSpeed = Speed;
						}
						break;

					case CollisionRuleType.Object:
						if (CollisionCandidates.Contains((PhysicalObject)cRule.Target) 
							&& Bounds.IntersectsWith(((PhysicalObject)cRule.Target).Bounds, Speed - ((PhysicalObject)cRule.Target).Speed, out touchingSpeedLeftFraction, out intersectionAxis)
							&& touchingSpeedLeftFraction > 0f
							&& touchingSpeedLeftFraction <= SpeedLeft
							&& (touchingSpeedLeftFraction <= ((PhysicalObject)cRule.Target).SpeedLeft 
							|| ((PhysicalObject)cRule.Target).Speed == Point.Zero)
							&& touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)
						{
							ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
							_closestCollisionTouchingAxis = intersectionAxis;
							_closestCollisionRule = cRule;
							_closestCollisionMethodArg = cRule.Target;
							_closestCollisionRelativeSpeed = Speed - ((PhysicalObject)cRule.Target).Speed;
						}
						break;

					case CollisionRuleType.ObjectType:
						foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[(Type)cRule.Target])
						{
							if (!CollisionCandidates.Contains(obj) ||
							    !Bounds.IntersectsWith(obj.Bounds, Speed - obj.Speed, out touchingSpeedLeftFraction, out intersectionAxis) ||
							    !(touchingSpeedLeftFraction > 0f) || !(touchingSpeedLeftFraction <= SpeedLeft) ||
							    (!(touchingSpeedLeftFraction <= obj.SpeedLeft) && obj.Speed != Point.Zero) ||
							    !(touchingSpeedLeftFraction < ClosestCollisionSpeedFraction)) 
								continue;

							ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
							_closestCollisionTouchingAxis = intersectionAxis;
							_closestCollisionRule = cRule;
							_closestCollisionMethodArg = obj;
							_closestCollisionRelativeSpeed = Speed - obj.Speed;
						}
						break;

					case CollisionRuleType.Objectmap:
						foreach (PhysicalObject obj in ((GameObjectCollection)cRule.Target).ActivePhysicalObjects)
						{
							//whether the object should be checked if the filter is enabled
							if (!cRule.FilterEnabled || cRule.ObjectmapFilter.Contains(obj.GetType())) //account for inheritance
							{
								if (!CollisionCandidates.Contains(obj) ||
								    !Bounds.IntersectsWith(obj.Bounds, Speed - obj.Speed, out touchingSpeedLeftFraction, out intersectionAxis) ||
								    !(touchingSpeedLeftFraction > 0f) || !(touchingSpeedLeftFraction <= SpeedLeft) ||
								    (!(touchingSpeedLeftFraction <= obj.SpeedLeft) && obj.Speed != Point.Zero) ||
								    !(touchingSpeedLeftFraction < ClosestCollisionSpeedFraction))
									continue;
								
								ClosestCollisionSpeedFraction = touchingSpeedLeftFraction * SpeedLeft;
								_closestCollisionTouchingAxis = intersectionAxis;
								_closestCollisionRule = cRule;
								_closestCollisionMethodArg = obj;
								_closestCollisionRelativeSpeed = Speed - obj.Speed;
							}
						}
						break;
				}
			}
		}

		/// <summary>
		/// Performs the calculated collision.
		/// pair determines whether both object's involved (if they are) will be performed, second one will be called with false to prevent loopdeeloops.
		/// </summary>
		internal void PerformClosestCollision(bool pair = true)
		{
			//what do you mean perform?
			if (SpeedLeft == 0f)
				return;

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

			switch (_closestCollisionRule.Action)
			{
				case CollisionAction.Block:
					Speed = 0f;
					SpeedLeft = 0f;
					break;

				case CollisionAction.Bounce:
					float speedDirection = SpeedDirection;
					float angleDifference = Misc.GetRelativeAngle(_closestCollisionRelativeSpeed.GetAngleFromOrigin(), speedDirection);

					//prevent from bouncing if it is getting hit from around the side or back (another object just hit this one)
					if (angleDifference > -60f && angleDifference < 60f)
						SpeedDirection = speedDirection - 180f + (_closestCollisionTouchingAxis.GetAngleFromOrigin() - speedDirection - 90f) * 2;
					break;
			}

			_closestCollisionRule.CallMethod(_closestCollisionMethodArg);

			if (_closestCollisionRule.DeactivateAfterCollision)
				_closestCollisionRule.Active = false;

			//make sure the other object (if any) performs his collision too before it gets rechecked
			if (!pair ||
			    (_closestCollisionRule.Type != CollisionRuleType.Object &&
			     _closestCollisionRule.Type != CollisionRuleType.Objectmap &&
			     _closestCollisionRule.Type != CollisionRuleType.ObjectType)) 
				return;

			//do not blindly assume the other object will collide against this one as well
			PhysicalObject otherObj = ((PhysicalObject)_closestCollisionMethodArg);
			if (otherObj._closestCollisionRule != null 
			    && (otherObj._closestCollisionRule.Type == CollisionRuleType.Object 
			        || otherObj._closestCollisionRule.Type == CollisionRuleType.Objectmap 
			        || otherObj._closestCollisionRule.Type == CollisionRuleType.ObjectType) 
			    && otherObj._closestCollisionMethodArg == this)
				otherObj.PerformClosestCollision(false);
		}

		#endregion

		public new void Destroy()
		{
			base.Destroy();

			Resources.PhysicalObjects.Remove(this);

			//remove from physicalobjectsbytype
			for (Type currentType = GetType(); currentType != typeof(GameObject); currentType = currentType.BaseType)
			{
				Resources.PhysicalObjectsByType[currentType].Remove(this);
			}
		}
	}
}