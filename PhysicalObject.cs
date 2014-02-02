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
		public Point Position = new Point(0, 0);
		public Point Speed = new Point(0, 0);
		private float _SpeedDirection = 0f;
		public float SpeedDirection
		{
			get 
			{
				//get the right direction when it cannot be calculated from the vector components
				if (Speed.X == 0 && Speed.Y == 0)
					return _SpeedDirection;
				return new Point(0, 0).AngleTo(Speed);
			}
			set 
			{ 
				float amplitude = new Point(0, 0).DistanceTo(Speed);
				Speed.X = (float)Math.Cos((value / 180 - 0.5) * Math.PI) * amplitude;
				Speed.Y = (float)Math.Sin((value / 180 - 0.5) * Math.PI) * amplitude;
			}
		}
		public float SpeedAmplitude
		{
			get 
			{
				return new Point(0, 0).DistanceTo(Speed);
			}
			set 
			{ 
				float direction = new Point(0, 0).AngleTo(Speed);
				Speed.X = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * value;
				Speed.Y = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * value;
			}
		}

		public Rectangle BoundBox;

		public PhysicalObject(Point position) : base()
        {
            //set position
            Position = position;
			BoundBox.Position = Position;

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
			Position += Speed;

            //update boundbox
			BoundBox.Position = Position;    
        }

		#region Crappy Collision checks
		public bool Collision(Rectangle rect)
        {
			return BoundBox.IntersectsWith(rect);
        }
		public bool Collision(Point point)
		{
			return BoundBox.IntersectsWith(point);
		}
		public bool Collision(PhysicalObject obj)
		{
			return BoundBox.IntersectsWith(obj.BoundBox);
		}
		public bool Collision(Type objType)
		{
			//perform all if actions are implemented, dont stop at the first
			foreach (PhysicalObject obj in Resources.PhysicalObjectsByType[objType])
			{
				if (BoundBox.IntersectsWith(obj.BoundBox))
					return true;
			}

			return false;
		}
		public bool Collision(ManagedSprite mSprite)
		{
			return BoundBox.IntersectsWith(mSprite.SpriteRectangle);
		}
		public bool Collision(string spritemapId, Point spritemapPos, string spriteNameFilter = "")
		{
			//perform all if actions are implemented, dont stop at the first
			Spritemap sMap = Resources.Spritemaps[spritemapId];
			foreach(ManagedSprite mSprite in sMap.ManagedSprites)
			{
				if (spriteNameFilter == "" || mSprite.TargetSprite.Id == spriteNameFilter)
				{
					Rectangle spriteRect = mSprite.SpriteRectangle;
					spriteRect.Position += spritemapPos;
					if (BoundBox.IntersectsWith(spriteRect))
						return true;
				}
			}
			return false;
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
