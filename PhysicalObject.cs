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
			/// Set object's speed to 0 at touching point.
			/// </summary>
			Block = 0,
			/// <summary>
			/// Bounce perfectly off the surface changing the SpeedDirection only.
			/// </summary>
			Bounce = 1,
			//Slide = 2, keep distance left or only distance on unlocked axis left?			
		}

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

		private List<Tuple<IShape, CollisionAction>> ShapeCollisionRules = new List<Tuple<IShape, CollisionAction>>();
		private List<Tuple<PhysicalObject, CollisionAction>> ObjectCollisionRules = new List<Tuple<PhysicalObject, CollisionAction>>();
		private List<Tuple<Type, CollisionAction>> ObjectTypeCollisionRules = new List<Tuple<Type, CollisionAction>>();
		private List<Tuple<ManagedSprite, CollisionAction>> ManagedSpriteCollisionRules = new List<Tuple<ManagedSprite, CollisionAction>>();
		private List<Tuple<Spritemap, CollisionAction, Point, Sprite>> SpritemapCollisionRules = new List<Tuple<Spritemap, CollisionAction, Point, Sprite>>();

		private List<Tuple<byte, int>> RemoveCollisionRules = new List<Tuple<byte, int>>();

        internal override void AfterStep()
        {
			SpeedLeft = 1f;
			do
			{
				float minTouchingFraction = float.PositiveInfinity;
				Point minTouchingAxis = Point.Zero;
				CollisionAction minTouchingAction = CollisionAction.Block;

				float ignoreFraction = 0f;
				Point ignoreAxis = Point.Zero;

				//so they don't have to be recreated ever goddem shape
				float touchingAtSpeedFraction;
				Point intersectionAxis;

				foreach(Tuple<IShape, CollisionAction> rule in ShapeCollisionRules)
				{
					if (Misc.ShapesIntersectingBySpeed(Bounds, rule.Item1, Speed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction < minTouchingFraction && !(touchingAtSpeedFraction == ignoreFraction && intersectionAxis == ignoreAxis))
					{
						minTouchingFraction = touchingAtSpeedFraction;
						minTouchingAxis = intersectionAxis;
						minTouchingAction = rule.Item2;
					}
				}
				foreach(Tuple<PhysicalObject, CollisionAction> rule in ObjectCollisionRules)
				{
					if (!rule.Item1.Destroyed)
					{
						if (Misc.ShapesIntersectingBySpeed(Bounds, rule.Item1.Bounds, rule.Item1.Speed - Speed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction < minTouchingFraction && !(touchingAtSpeedFraction == ignoreFraction && intersectionAxis == ignoreAxis))
						{
							minTouchingFraction = touchingAtSpeedFraction;
							minTouchingAxis = intersectionAxis;
							minTouchingAction = rule.Item2;
						}
					}
					else
					{
						//remove from 
						RemoveCollisionRules.Add(new Tuple<byte, int>(1, ObjectCollisionRules.IndexOf(rule)));
					}
				}
				foreach(Tuple<Type, CollisionAction> rule in ObjectTypeCollisionRules)
				{
					foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[rule.Item1])
					{
						if (Misc.ShapesIntersectingBySpeed(Bounds, obj.Bounds, obj.Speed - Speed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction < minTouchingFraction && !(touchingAtSpeedFraction == ignoreFraction && intersectionAxis == ignoreAxis))
						{
							minTouchingFraction = touchingAtSpeedFraction;
							minTouchingAxis = intersectionAxis;
							minTouchingAction = rule.Item2;
						}
					}
				}
				foreach(Tuple<ManagedSprite, CollisionAction> rule in ManagedSpriteCollisionRules)
				{
					if (Misc.ShapesIntersectingBySpeed(Bounds, rule.Item1.SpriteRectangle, Speed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction < minTouchingFraction && !(touchingAtSpeedFraction == ignoreFraction && intersectionAxis == ignoreAxis))
					{
						minTouchingFraction = touchingAtSpeedFraction;
						minTouchingAxis = intersectionAxis;
						minTouchingAction = rule.Item2;
					}
				}
				foreach(Tuple<Spritemap, CollisionAction, Point, Sprite> rule in SpritemapCollisionRules)
				{
					foreach(ManagedSprite sprite in rule.Item1.ManagedSprites)
					{
						Rectangle sRect = sprite.SpriteRectangle;
						sRect.Position += rule.Item3;
						if (Misc.ShapesIntersectingBySpeed(Bounds, sRect, Speed, out touchingAtSpeedFraction, out intersectionAxis) && touchingAtSpeedFraction < minTouchingFraction && !(touchingAtSpeedFraction == ignoreFraction && intersectionAxis == ignoreAxis))
						{
							minTouchingFraction = touchingAtSpeedFraction;
							minTouchingAxis = intersectionAxis;
							minTouchingAction = rule.Item2;
						}
					}
				}

				//collision is within range, perform it
				if (minTouchingFraction < SpeedLeft)
				{
					//move to touching point
					Position += Speed * minTouchingFraction;
					SpeedLeft -= minTouchingFraction;

					if (minTouchingAction == CollisionAction.Block)
					{
						Speed = 0f;
						SpeedLeft = 0f;
					}
					if (minTouchingAction == CollisionAction.Bounce)
					{
						SpeedDirection = Speed.Angle - 180 + (minTouchingAxis.Angle - Speed.Angle) * 2;
					}

					ignoreFraction = minTouchingFraction;
					ignoreAxis = minTouchingAxis;
				}
				else //move the remaining distance
				{
					Position += Speed * SpeedLeft;
					SpeedLeft = 0f;
				}
			} while (SpeedLeft > 0f);

			//remove collisionrules
			foreach(Tuple<byte, int> rule in RemoveCollisionRules)
			{
				switch(rule.Item1)
				{
					case 0:
						ShapeCollisionRules.RemoveAt(rule.Item2);
						break;
					case 1:
						ObjectCollisionRules.RemoveAt(rule.Item2);
						break;
					case 2:
						ObjectTypeCollisionRules.RemoveAt(rule.Item2);
						break;
					case 3:
						ManagedSpriteCollisionRules.RemoveAt(rule.Item2);
						break;
					case 4:
						SpritemapCollisionRules.RemoveAt(rule.Item2);
						break;
				}
			}
			RemoveCollisionRules.Clear();
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

		/// <summary>
		/// Add rule for a static shape.
		/// </summary>
		public void AddCollisionRule(IShape shape, CollisionAction action, bool thisStepOnly = false)
		{
			Tuple<IShape, CollisionAction> rule = new Tuple<IShape, CollisionAction>(shape, action);
			ShapeCollisionRules.Add(rule);

			if (thisStepOnly)
				RemoveCollisionRules.Add(new Tuple<byte, int>(0, ShapeCollisionRules.IndexOf(rule)));
		}
		/// <summary>
		/// Add rule for a specific object.
		/// </summary>
		public void AddCollisionRule(PhysicalObject obj, CollisionAction action, bool thisStepOnly = false)
		{
			Tuple<PhysicalObject, CollisionAction> rule = new Tuple<PhysicalObject, CollisionAction>(obj, action);
			ObjectCollisionRules.Add(rule);

			if (thisStepOnly)
				RemoveCollisionRules.Add(new Tuple<byte, int>(1, ObjectCollisionRules.IndexOf(rule)));
		}
		/// <summary>
		/// Add rule for all objects of a type.
		/// </summary>
		public void AddCollisionRule(Type objType, CollisionAction action, bool thisStepOnly = false)
		{
			if (!objType.IsAssignableFrom(typeof(PhysicalObject)))
				throw new ArgumentException("Type is not derived from PhysicalObject");

			Tuple<Type, CollisionAction> rule = new Tuple<Type, CollisionAction>(objType, action);
				ObjectTypeCollisionRules.Add(rule);

			if (thisStepOnly)
				RemoveCollisionRules.Add(new Tuple<byte, int>(2, ObjectTypeCollisionRules.IndexOf(rule)));
		}
		/// <summary>
		/// Add rule for a ManagedSprite.
		/// </summary>
		public void AddCollisionRule(ManagedSprite mSprite, CollisionAction action, bool thisStepOnly = false)
		{
			Tuple<ManagedSprite, CollisionAction> rule = new Tuple<ManagedSprite, CollisionAction>(mSprite, action);
				ManagedSpriteCollisionRules.Add(rule);

			if (thisStepOnly)
				RemoveCollisionRules.Add(new Tuple<byte, int>(3, ManagedSpriteCollisionRules.IndexOf(rule)));
		}
		/// <summary>
		/// Add rule for all sprites in a spritemap with a specified position.
		/// If spriteIdFilter is not an empty string it will only check for sprites with this id.
		/// </summary>
		public void AddCollisionRule(string spritemapId, Point spritemapPos, CollisionAction action, string spriteIdFilter = "", bool thisStepOnly = false)
		{
			Sprite spriteFilter = null;
			if (spriteIdFilter != "")
				spriteFilter = Resources.Sprites[spriteIdFilter];

			Tuple<Spritemap, CollisionAction, Point, Sprite> rule = new Tuple<Spritemap, CollisionAction, Point, Sprite>(Resources.Spritemaps[spritemapId], action, spritemapPos, spriteFilter);
			SpritemapCollisionRules.Add(rule);  

			if (thisStepOnly)
				RemoveCollisionRules.Add(new Tuple<byte, int>(4, SpritemapCollisionRules.IndexOf(rule)));
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
