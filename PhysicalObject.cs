using System;

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
            BOUNCEBOTH = 3,
        }

        public float X;
        public float Y;
        public float Width = 0;
        public float Height = 0;

        private float BoundBoxX;
        private float BoundBoxY;
        private float BoundBoxHSpeed;
        private float BoundBoxVSpeed;
        public float BoundBoxXOffset = 0;
        public float BoundBoxYOffset = 0;
        public float BoundBoxWidth = 0;
        public float BoundBoxHeight = 0;

        public Sprite BuiltinSprite = null;
        public uint BuiltinSpriteIndex = 0;
        public AnimatedSprite BuiltinAnimatedSprite = null;

        private float direction = 0;
        private float speed = 0;
        private float hSpeed = 0;
        private float vSpeed = 0;
        public float Direction
        {
            get { return direction; }
            set
            {
                while (value >= 360)
                    value = value - 360;
                while (value < 0)
                    value = value + 360;
                direction = value;

                hSpeed = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * speed;
                vSpeed = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * speed;
            }
        }
        public float Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                hSpeed = (float)Math.Cos((direction / 180 - 0.5) * Math.PI) * speed;
                vSpeed = (float)Math.Sin((direction / 180 - 0.5) * Math.PI) * speed;
            }
        }
        public float HSpeed
        {
            get { return hSpeed; }
            set
            {
                hSpeed = value;
                speed = (float)Math.Sqrt(Math.Pow(hSpeed, 2) / Math.Pow(vSpeed, 2));
                direction = (float)(Math.Atan2(hSpeed, vSpeed) / Math.PI * 180);
            }
        }
        public float VSpeed
        {
            get { return vSpeed; }
            set
            {
                vSpeed = value;
                speed = (float)Math.Sqrt(Math.Pow(hSpeed, 2) / Math.Pow(vSpeed, 2));
                direction = (float)(Math.Atan2(hSpeed, vSpeed) / Math.PI * 180);
            }
        }

        internal override void BeforeStep()
        {
        }

        internal override void AfterStep()
        {
            //move
            X += hSpeed;
            Y += vSpeed;

            //move boundbox
            BoundBoxHSpeed = hSpeed;
            BoundBoxVSpeed = vSpeed;
            BoundBoxX = X + BoundBoxXOffset;
            BoundBoxY = Y + BoundBoxYOffset;

            //update built-in animated sprite
            if (BuiltinAnimatedSprite != null)
                BuiltinAnimatedSprite.Update();
        }

        internal override void AfterDraw(float stepProgress)
        {
            //draw built-in sprite and animated sprite
            if (BuiltinSprite != null)
                BuiltinSprite.Draw(X - hSpeed * (1 - stepProgress), Y - vSpeed * (1 - stepProgress), BuiltinSpriteIndex);
            if (BuiltinAnimatedSprite != null)
                BuiltinAnimatedSprite.Draw(X - hSpeed * (1 - stepProgress), Y - vSpeed * (1 - stepProgress));
        }

        public bool Collision(Rectangle rectangle, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            bool result = false;

            //check if this object's boundbox has passed the rectangle from the top or bottom this step
            if (BoundBoxX + BoundBoxWidth >= rectangle.X && BoundBoxX <= rectangle.X + rectangle.Width)
            {
                if ((side == CollisionSide.BOTTOM || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxY + BoundBoxHeight >= rectangle.Y && BoundBoxY + BoundBoxHeight - BoundBoxVSpeed <= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Y = rectangle.Y - BoundBoxHeight;
                        VSpeed = Math.Min(0, VSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Y = 2 * rectangle.Y - 2 * BoundBoxHeight - BoundBoxY;
                        VSpeed = -VSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOP || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxY <= rectangle.Y + rectangle.Height && BoundBoxY - BoundBoxVSpeed >= rectangle.Y + rectangle.Height)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Y = rectangle.Y + rectangle.Height;
                        VSpeed = Math.Max(0, VSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Y = 2 * rectangle.Y + 2 * rectangle.Height - BoundBoxY;
                        VSpeed = -VSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.BOTTOMINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL) 
                    && BoundBoxY + BoundBoxHeight >= rectangle.Y + rectangle.Height && BoundBoxY + BoundBoxHeight - BoundBoxVSpeed <= rectangle.Y + rectangle.Height)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Y = rectangle.Y + rectangle.Height - BoundBoxHeight;
                        VSpeed = Math.Min(0, VSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Y = 2 * rectangle.Y + 2 * rectangle.Height - 2 * BoundBoxHeight - BoundBoxY;
                        VSpeed = -VSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.TOPINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxY <= rectangle.Y && BoundBoxY - BoundBoxVSpeed >= rectangle.Y)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        Y = rectangle.Y;
                        VSpeed = Math.Max(0, VSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        Y = rectangle.Y + rectangle.Y - BoundBoxY;
                        VSpeed = -VSpeed;
                    }
                    result = true;
                }
            }

            //left or right
            if (BoundBoxY + BoundBoxHeight >= rectangle.Y && BoundBoxY <= rectangle.Y + rectangle.Height)
            {
                if ((side == CollisionSide.RIGHT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxX + BoundBoxWidth >= rectangle.X && BoundBoxX + BoundBoxWidth - BoundBoxHSpeed <= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        X = rectangle.X - BoundBoxWidth;
                        HSpeed = Math.Min(0, HSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        X = 2 * rectangle.X - 2 * BoundBoxWidth - BoundBoxX;
                        HSpeed = -HSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFT || side == CollisionSide.ALLOUTSIDE || side == CollisionSide.ALL)
                    && BoundBoxX <= rectangle.X + rectangle.Width && BoundBoxX - BoundBoxHSpeed >= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        X = rectangle.X + rectangle.Width;
                        HSpeed = Math.Max(0, HSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        X = 2 * rectangle.X + 2 * rectangle.Width - BoundBoxX;
                        HSpeed = -HSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.RIGHTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxX + BoundBoxWidth >= rectangle.X + rectangle.Width && BoundBoxX + BoundBoxWidth - BoundBoxHSpeed <= rectangle.X + rectangle.Width)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        X = rectangle.X + rectangle.Width - BoundBoxWidth;
                        HSpeed = Math.Min(0, HSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        X = 2 * rectangle.X + 2 * rectangle.Width - 2 * BoundBoxWidth - BoundBoxX;
                        HSpeed = -HSpeed;
                    }
                    result = true;
                }
                if ((side == CollisionSide.LEFTINSIDE || side == CollisionSide.ALLINSIDE || side == CollisionSide.ALL)
                    && BoundBoxX <= rectangle.X && BoundBoxX - BoundBoxHSpeed >= rectangle.X)
                {
                    if (action == CollisionAction.BLOCK)
                    {
                        X = rectangle.X;
                        HSpeed = Math.Max(0, HSpeed);
                    }
                    if (action == CollisionAction.BOUNCE)
                    {
                        X = 2 * rectangle.X - BoundBoxX;
                        HSpeed = -HSpeed;
                    }
                    result = true;
                }
            }

            return result;
        }

        public bool Collision(PhysicalObject checkObject, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            //temp solution, perm solution will account for other object's movement
            return Collision(new Rectangle(checkObject.BoundBoxX, checkObject.BoundBoxY, checkObject.BoundBoxWidth, checkObject.BoundBoxHeight), side, action);
        }

        public bool Collision(Type checkObjectType, CollisionSide side = CollisionSide.ALLOUTSIDE, CollisionAction action = CollisionAction.NONE)
        {
            if (!typeof(PhysicalObject).IsAssignableFrom(checkObjectType))
                Log.WriteLine(checkObjectType.ToString() + " does not inherit from PhysicalObject.", ErrorLevel.FATAL);

            bool result = false;

            foreach (PhysicalObject object2 in Game.PhysicalObjectsByType[checkObjectType])
            {
                if (Collision(object2, side, action))
                    result = true;
            }

            return result;
        }
    }
}
