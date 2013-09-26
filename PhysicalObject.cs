using System;

namespace HatlessEngine
{
    public class PhysicalObject : LogicalObject
    {
        public float X;
        public float Y;

        private float BoundBoxX;
        private float BoundBoxY;
        private float BoundBoxHSpeed;
        private float BoundBoxVSpeed;
        public float BoundBoxXOffset = 0;
        public float BoundBoxYOffset = 0;
        public float BoundBoxWidth = 0;
        public float BoundBoxHeight = 0;

        public Sprite sprite = null;

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

        internal void UpdateBoundBox()
        {
            BoundBoxHSpeed = hSpeed;
            BoundBoxVSpeed = vSpeed;
            BoundBoxX = X + BoundBoxXOffset;
            BoundBoxY = Y + BoundBoxYOffset;
        }

        internal void UpdatePosition()
        {
            X += hSpeed;
            Y += vSpeed;
        }

        internal void DrawSprite(float stepProgress)
        {
            if (sprite != null)
                sprite.Draw(X + hSpeed * stepProgress, Y + vSpeed * stepProgress);
        }

        public Tuple<bool, PhysicalObject, float> CheckCollision<T>(Side side = Side.ALL) where T : PhysicalObject
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

                    //check overlapping, then relative direction for side
                    if (((BoundBoxX > object2.BoundBoxX && BoundBoxX < object2.BoundBoxX + object2.BoundBoxWidth) ||                                         //this X1 within object2 dX
                        (BoundBoxX + BoundBoxWidth > object2.BoundBoxX && BoundBoxX + BoundBoxWidth < object2.BoundBoxX + object2.BoundBoxWidth) ||         //this X2 within object2 dX
                        (object2.BoundBoxX > BoundBoxX && object2.BoundBoxX < BoundBoxX + BoundBoxWidth) ||                                                 //object2 X1 within this dX
                        (object2.BoundBoxX + object2.BoundBoxWidth > BoundBoxX && object2.BoundBoxX + object2.BoundBoxWidth < BoundBoxX + BoundBoxWidth)) &&  //object2 X2 within this dX
                       ((BoundBoxY > object2.BoundBoxY && BoundBoxY < object2.BoundBoxY + object2.BoundBoxHeight) ||                                         
                        (BoundBoxY + BoundBoxHeight > object2.BoundBoxY && BoundBoxY + BoundBoxHeight < object2.BoundBoxY + object2.BoundBoxHeight) ||         
                        (object2.BoundBoxY > BoundBoxY && object2.BoundBoxY < BoundBoxY + BoundBoxHeight) ||                                               
                        (object2.BoundBoxY + object2.BoundBoxHeight > BoundBoxY && object2.BoundBoxY + object2.BoundBoxHeight < BoundBoxY + BoundBoxHeight)))
                    {
                        return new Tuple<bool, PhysicalObject, float>(true, object2, 0);
                    }
                }

                //no collision with given type
                return new Tuple<bool, PhysicalObject, float>(false, null, 0);
            }
            else
                return new Tuple<bool, PhysicalObject, float>(false, null, 0);
        }
    }
}
