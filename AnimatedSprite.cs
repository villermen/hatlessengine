using System;

namespace HatlessEngine
{
    /// <summary>
    /// Can be used to draw animations of a sprite without having to manage pointers yourself.
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
                if (!TargetSprite.Animations.ContainsKey(value))
                    Log.WriteLine("AnimatedSprite: Sprite '" + TargetSprite.ToString() + "' does not have an animation with id '" + value + "'", ErrorLevel.FATAL);
                animationId = value;
                Stepnumber = 0;
                IndexIncrements = 0;
            }
        }
        public float Speed = 0;
        private long Stepnumber = 0;
        private long IndexIncrements = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetSprite">Sprite object from Resources to use.</param>
        /// <param name="animationId">The sprite's animation id string, can be changed using AnimationId after this.</param>
        public AnimatedSprite(Sprite targetSprite, string animationId)
        {
            TargetSprite = targetSprite;
            AnimationId = animationId;
        }

        public void Draw(float x, float y)
        {
            TargetSprite.Draw(x, y, CurrentIndex);
        }

        /// <summary>
        /// Makes the animation actually work, call this on every step.
        /// (BuiltinAnimation will have this done automagically)
        /// </summary>
        public void Update()
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
