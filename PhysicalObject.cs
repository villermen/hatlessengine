using System;

namespace HatlessEngine
{
    /// <summary>
    /// Object that has a physical position and size in the game.
    /// Has built-in support for a sprite, boundbox etc.
    /// </summary>
    public class PhysicalObject : LogicalObject
    {
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
            BoundBoxHSpeed = hSpeed;
            BoundBoxVSpeed = vSpeed;
            BoundBoxX = X + BoundBoxXOffset;
            BoundBoxY = Y + BoundBoxYOffset;
        }

        internal override void AfterStep()
        {
            //move
            X += hSpeed;
            Y += vSpeed;

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
                BuiltinSprite.Draw(X + hSpeed * stepProgress, Y + vSpeed * stepProgress, BuiltinSpriteIndex);
            if (BuiltinAnimatedSprite != null)
                BuiltinAnimatedSprite.Draw(X + hSpeed * stepProgress, Y + vSpeed * stepProgress);
        }

        public bool CheckCollision<T>(Side side = Side.ALL, bool correctPosition = false) where T : PhysicalObject
        {
            Type type = typeof(T);
            if (Game.PhysicalObjectsByType.ContainsKey(type))
            {
                //check against all registered objects of the given type
                foreach (PhysicalObject object2 in Game.PhysicalObjectsByType[type])
                {
                    //where the current object is moving to relative to the second one (for checking whether its a collision or an overlap)
                    float hSpeedDifference = BoundBoxHSpeed - object2.BoundBoxHSpeed;
                    float vSpeedDifference = BoundBoxVSpeed - object2.BoundBoxVSpeed;

                    //check if this object's boundbox has passed the other object's boundbox from the top or bottom this step
                    if (BoundBoxX + BoundBoxWidth >= object2.BoundBoxX && BoundBoxX <= object2.BoundBoxX + object2.BoundBoxWidth)
                    {
                        if ((side == Side.BOTTOM || side == Side.ALL) && BoundBoxY + BoundBoxHeight >= object2.BoundBoxY && BoundBoxY + BoundBoxHeight - BoundBoxVSpeed <= object2.BoundBoxY)
                        {
                            if (correctPosition)
                                Y = object2.BoundBoxY - BoundBoxHeight - BoundBoxYOffset;
                            return true;
                        }
                        if ((side == Side.TOP || side == Side.ALL) && BoundBoxY <= object2.BoundBoxY + object2.BoundBoxHeight && BoundBoxY - BoundBoxVSpeed >= object2.BoundBoxY + object2.BoundBoxHeight)
                        {
                            if (correctPosition)
                                Y = object2.BoundBoxY + object2.BoundBoxHeight - BoundBoxYOffset;
                            return true;
                        }
                    }

                    //left or right
                    if (BoundBoxY + BoundBoxHeight >= object2.BoundBoxY && BoundBoxY <= object2.BoundBoxY + object2.BoundBoxHeight)
                    {
                        if ((side == Side.RIGHT || side == Side.ALL) && BoundBoxX + BoundBoxWidth >= object2.BoundBoxX && BoundBoxX + BoundBoxWidth - BoundBoxHSpeed <= object2.BoundBoxX)
                        {
                            if (correctPosition)
                                X = object2.BoundBoxX - BoundBoxWidth - BoundBoxXOffset;
                            return true;
                        }
                        if ((side == Side.RIGHT || side == Side.ALL) && BoundBoxX <= object2.BoundBoxX + object2.BoundBoxWidth && BoundBoxX - BoundBoxHSpeed >= object2.BoundBoxX + object2.BoundBoxWidth)
                        {
                            if (correctPosition)
                                X = object2.BoundBoxX + object2.BoundBoxWidth - BoundBoxXOffset;
                            return true;
                        }
                    }
                }

                //no collision with given type
                return false;
            }
            else
                return false;
        }
    }
}
