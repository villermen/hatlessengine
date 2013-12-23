using System;
using System.Drawing;

namespace HatlessEngine
{
    /// <summary>
    /// Can be used to draw animations of a sprite without having to manage pointers yourself.
    /// Must be initialized per animation, unlike external resources.
    /// </summary>
    public class AnimatedSprite
    {
        public uint CurrentIndex = 0;
        public Sprite TargetSprite { get; private set; }
        private string animationId = "";
        public string AnimationId
        {
            get { return animationId; }
            set
            {
                animationId = value;
                Stepnumber = 0;
                IndexIncrements = 0;
            }
        }
        /// <summary>
        /// Frames per step, use SetFramesPerSecond if you don't like maths.
        /// </summary>
        public float Speed = 1;
        private long Stepnumber = 0;
        private long IndexIncrements = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetSprite">Sprite object id from Resources to use.</param>
        /// <param name="animationId">The sprite's animation id string, can be changed using AnimationId after this.</param>
        public AnimatedSprite(string targetSpriteId, string animationId)
        {
            TargetSprite = Resources.Sprites[targetSpriteId];
            AnimationId = animationId;
        }

		public void Draw(PointF pos)
        {
            TargetSprite.Draw(pos, CurrentIndex);
        }

        /// <summary>
        /// Makes the animation actually work, call this on every step.
        /// (BuiltinAnimation will have this done automagically)
        /// </summary>
        public void Step()
        {
            if (IndexIncrements + 1 <= Stepnumber * Speed)
            {
                if (CurrentIndex < TargetSprite.Animations[AnimationId].Length - 1)
                    CurrentIndex++;
                else
                    CurrentIndex = 0;

                IndexIncrements++;
            }

            Stepnumber++;
        }

        /// <summary>
        /// Sets speed per second instead of per step.
        /// </summary>
        /// <param name="fps">Amount of frames yer be wanting.</param>
        public void SetFramesPerSecond(float fps)
        {
            Speed = 1 / (Game.Speed / fps);
        }
    }
}
