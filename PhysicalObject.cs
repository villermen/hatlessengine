﻿using System;
using System.Collections.Generic;

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

        public Position Position = new Position(0, 0);
        public Speed Speed = new Speed(0, 0);

        public Rectangle BoundBoxRectangle;
        public Speed BoundBoxSpeed;
        public Position BoundBoxOffset;

        public PhysicalObject(Position position) : base()
        {
            //set position
            Position = position;
            BoundBoxRectangle.Position = Position;

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
            BoundBoxRectangle.X = Position.X + BoundBoxOffset.X;
            BoundBoxRectangle.Y = Position.Y + BoundBoxOffset.Y;
            BoundBoxSpeed = Speed;          
        }

        public bool Collision(Rectangle rectangle, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            bool result = false;

            //check if this object's boundbox has passed the rectangle from the top or bottom this step
            if (BoundBoxRectangle.X2 >= rectangle.X && BoundBoxRectangle.X <= rectangle.X2)
            {
                if ((side == CollisionSide.BOTTOM || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.Y2 >= rectangle.Y && BoundBoxRectangle.Y2 - BoundBoxSpeed.Y <= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y - BoundBoxRectangle.Height;
                        Speed.Y = Math.Min(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y - 2 * BoundBoxRectangle.Height - BoundBoxRectangle.Y;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOP || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.Y <= rectangle.Y2 && BoundBoxRectangle.Y - BoundBoxSpeed.Y >= rectangle.Y2)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y2;
                        Speed.Y = Math.Max(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y2 - BoundBoxRectangle.Y;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.BOTTOMINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL) 
                    && BoundBoxRectangle.Y2 >= rectangle.Y2 && BoundBoxRectangle.Y2 - BoundBoxSpeed.Y <= rectangle.Y2)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y2 - BoundBoxRectangle.Height;
                        Speed.Y = Math.Min(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y2 - 2 * BoundBoxRectangle.Height - BoundBoxRectangle.Y;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOPINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.Y <= rectangle.Y && BoundBoxRectangle.Y - BoundBoxSpeed.Y >= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.Y = rectangle.Y;
                        Speed.Y = Math.Max(0, Speed.Y);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.Y = 2 * rectangle.Y - BoundBoxRectangle.Y;
                        Speed.Y = -Speed.Y;
                    }
                    result = true;
                }
            }

            //left or right
            if (BoundBoxRectangle.Y2 >= rectangle.Y && BoundBoxRectangle.Y <= rectangle.Y2)
            {
                if ((side == CollisionSide.RIGHT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.X2 >= rectangle.X && BoundBoxRectangle.X2 - BoundBoxSpeed.X <= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X - BoundBoxRectangle.Width;
                        Speed.X = Math.Min(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X - 2 * BoundBoxRectangle.Width - BoundBoxRectangle.X;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.X <= rectangle.X + rectangle.Width && BoundBoxRectangle.X - BoundBoxSpeed.X >= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X2;
                        Speed.X = Math.Max(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X2 - BoundBoxRectangle.X;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.RIGHTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.X2 >= rectangle.X2 && BoundBoxRectangle.X2 - BoundBoxSpeed.X <= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X + rectangle.Width - BoundBoxRectangle.Width;
                        Speed.X = Math.Min(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X + 2 * rectangle.Width - 2 * BoundBoxRectangle.Width - BoundBoxRectangle.X;
                        Speed.X = -Speed.X;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxRectangle.X <= rectangle.X && BoundBoxRectangle.X - BoundBoxSpeed.X >= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Position.X = rectangle.X;
                        Speed.X = Math.Max(0, Speed.X);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Position.X = 2 * rectangle.X - BoundBoxRectangle.X;
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
            return Collision(checkObject.BoundBoxRectangle, side, action);
        }
        public BoolAndData<List<PhysicalObject>> Collision(Type checkObjectType, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            List<PhysicalObject> resultObjects = new List<PhysicalObject>();

            foreach (PhysicalObject object2 in Resources.PhysicalObjectsByType[checkObjectType])
            {
                if (Collision(object2, side, action))
                    resultObjects.Add(object2);
            }

            if (resultObjects.Count > 0)
                return new BoolAndData<List<PhysicalObject>>(true, resultObjects);
            else
                return new BoolAndData<List<PhysicalObject>>(false, null);
        }
        /// <summary>
        /// Checks collision against every sprite in a spritemap with position pos.
        /// </summary>
        public BoolAndData<List<SpritemapBlueprint>> Collision(string spritemapId, Position pos, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            List<SpritemapBlueprint> resultSprites = new List<SpritemapBlueprint>();

            foreach (SpritemapBlueprint blueprint in Resources.Spritemaps[spritemapId].Blueprints)
            {
                if (Collision(blueprint.Rectangle + pos, side, action))
                    resultSprites.Add(blueprint);
            }

            if (resultSprites.Count > 0)
                return new BoolAndData<List<SpritemapBlueprint>>(true, resultSprites);
            else
                return new BoolAndData<List<SpritemapBlueprint>>(false, null);
        }
        /// <summary>
        /// Checks collision against a certain sprite in a spritemap with position pos.
        /// </summary>
        public BoolAndData<List<SpritemapBlueprint>> Collision(string spritemapId, Position pos, string spriteId, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            List<SpritemapBlueprint> resultSprites = new List<SpritemapBlueprint>();

            foreach (SpritemapBlueprint blueprint in Resources.Spritemaps[spritemapId].Blueprints)
            {
                if (blueprint.Sprite.Id == spriteId)
                {
                    if (Collision(blueprint.Rectangle + pos, side, action))
                        resultSprites.Add(blueprint);
                }
            }

            if (resultSprites.Count > 0)
                return new BoolAndData<List<SpritemapBlueprint>>(true, resultSprites);
            else
                return new BoolAndData<List<SpritemapBlueprint>>(false, null);
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
