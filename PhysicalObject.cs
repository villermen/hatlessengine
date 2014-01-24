using System;
using System.Collections.Generic;
using System.Drawing;

namespace HatlessEngine
{
    /// <summary>
    /// Object that has a physical position and size in the game.
    /// Has built-in support for a sprite, boundbox etc.
    /// </summary>
    public class PhysicalObject : LogicalObject
    {
        public enum CollisionSide
        {
            ALL = 1,
            ALLOUTSIDE = 2,
            ALLINSIDE = 3,

            TOP = 4,
            RIGHT = 5,
            BOTTOM = 6,
            LEFT = 7,

            TOPINSIDE = 8,
            RIGHTINSIDE = 9,
            BOTTOMINSIDE = 10,
            LEFTINSIDE = 11,
        }
        public enum CollisionAction
        {
            NONE = 0,
            BLOCK = 1,
            BOUNCE = 2,
            //BOUNCEBOTH = 3, or weightedbounce
        }
		
		public PointF Position = new PointF(0, 0);
		public PointF Speed = new PointF(0, 0);
		private float _SpeedDirection = 0f;
		public float SpeedDirection
		{
			get 
			{
				//get the right direction when it cannot be calculated from the vector components
				if (Speed.X == 0 && Speed.Y == 0)
					return _SpeedDirection;
				return Misc.AngleBetweenPoints(new PointF(0, 0), Speed);
			}
			set 
			{ 
				float amplitude = Misc.DistanceBetweenPoints(new PointF(0, 0), Speed);
				Speed.X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * amplitude;
				Speed.Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * amplitude;
			}
		}
		public float SpeedAmplitude
		{
			get 
			{
				return Misc.DistanceBetweenPoints(new PointF(0, 0), Speed);
			}
			set 
			{ 
				float direction = Misc.AngleBetweenPoints(new PointF(0, 0), Speed);
				Speed.X = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * value;
				Speed.Y = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * value;
			}
		}

		public RectangleF BoundBox;

		public PhysicalObject(PointF position) : base()
        {
            //set position
            Position = position;
			BoundBox.Location = Position;

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
            //move
			Position.X += Speed.X;
			Position.Y += Speed.Y;

            //update boundbox
			BoundBox.Location = Position;    
        }

		public bool Collision(RectangleF rectangle, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            bool result = false;

            //check if this object's boundbox has passed the rectangle from the top or bottom this step
			if (BoundBox.Right >= rectangle.Left && BoundBox.Left <= rectangle.Right)
            {
                if ((side == CollisionSide.BOTTOM || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
					&& BoundBox.Bottom >= rectangle.Y && BoundBox.Bottom - Speed.Y <= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y - BoundBox.Height;
                        Speed.Y = Math.Min(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y - 2 * BoundBox.Height - BoundBox.Top;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOP || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
					&& BoundBox.Top <= rectangle.Bottom && BoundBox.Top - Speed.Y >= rectangle.Bottom)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Bottom;
                        Speed.Y = Math.Max(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Bottom - BoundBox.Top;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.BOTTOMINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL) 
					&& BoundBox.Bottom >= rectangle.Bottom && BoundBox.Bottom - Speed.Y <= rectangle.Bottom)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Bottom - BoundBox.Height;
                        Speed.Y = Math.Min(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Bottom - 2 * BoundBox.Height - BoundBox.Top;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOPINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
					&& BoundBox.Top <= rectangle.Y && BoundBox.Top - Speed.Y >= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y;
                        Speed.Y = Math.Max(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y - BoundBox.Top;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
            }

            //left or right
            if (BoundBox.Bottom >= rectangle.Y && BoundBox.Top <= rectangle.Bottom)
            {
                if ((side == CollisionSide.RIGHT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBox.Right >= rectangle.X && BoundBox.Right - Speed.X <= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X - BoundBox.Width;
                        Speed.X = Math.Min(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X - 2 * BoundBox.Width - BoundBox.Left;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBox.Left <= rectangle.X + rectangle.Width && BoundBox.Left - Speed.X >= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.Right;
                        Speed.X = Math.Max(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.Right - BoundBox.Left;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.RIGHTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBox.Right >= rectangle.Right && BoundBox.Right - Speed.X <= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X + rectangle.Width - BoundBox.Width;
                        Speed.X = Math.Min(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X + 2 * rectangle.Width - 2 * BoundBox.Width - BoundBox.Left;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBox.Left <= rectangle.X && BoundBox.Left - Speed.X >= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X;
                        Speed.X = Math.Max(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X - BoundBox.Left;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
            }

            return result;
        }
        public bool Collision(PhysicalObject checkObject, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            //temp solution, perm solution will account for other object's movement
            if (checkObject.Destroyed)
                return false;
            return Collision(checkObject.BoundBox, side, action);
        }
		public bool Collision(Type checkObjectType, out List<PhysicalObject> objects, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
			objects = new List<PhysicalObject>();

            foreach (PhysicalObject object2 in Resources.PhysicalObjectsByType[checkObjectType])
            {
                if (Collision(object2, side, action))
					objects.Add(object2);
            }

			return objects.Count > 0 ? true : false;
        }
        /// <summary>
		/// Checks collision against every sprite in a spritemap with position pos.
        /// </summary>
		/*public bool Collision(string spritemapId, PointF pos, out List<SpritemapBlueprint> sprites, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
			sprites = new List<SpritemapBlueprint>();

            foreach (SpritemapBlueprint blueprint in Resources.Spritemaps[spritemapId].Blueprints)
            {
				RectangleF absRect = new RectangleF(blueprint.Rectangle.X + pos.X, blueprint.Rectangle.Y + pos.Y, blueprint.Rectangle.Width, blueprint.Rectangle.Height);
				if (Collision(absRect, side, action))
					sprites.Add(blueprint);
            }

			return sprites.Count > 0 ? true : false;
        }
        /// <summary>
        /// Checks collision against a certain sprite in a spritemap with position pos.
        /// </summary>
		public bool Collision(string spritemapId, PointF pos, string spriteId, out List<SpritemapBlueprint> sprites, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
			sprites = new List<SpritemapBlueprint>();

            foreach (SpritemapBlueprint blueprint in Resources.Spritemaps[spritemapId].Blueprints)
            {
                if (blueprint.Sprite.Id == spriteId)
                {
					RectangleF absRect = new RectangleF(blueprint.Rectangle.X + pos.X, blueprint.Rectangle.Y + pos.Y, blueprint.Rectangle.Width, blueprint.Rectangle.Height);
					if (Collision(absRect, side, action))
						sprites.Add(blueprint);
                }
            }

			return sprites.Count > 0 ? true : false;
		}*/

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
