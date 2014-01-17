using System;
using System.Drawing;

namespace HatlessEngine
{
    /// <summary>
	/// Holds parameters for drawing a sprite so you don't have to store those for yourself for the sprite.
	/// See it as an instance of a sprite that can be drawn.
    /// </summary>
    public class ManagedSprite
	{
		public Sprite TargetSprite { get; private set; }

		public PointF Position;

		public PointF Scale;

		public PointF Origin;
		public float Rotation;

		public bool UseCustomDepth = false;
		private sbyte _Depth = 0;
		public sbyte Depth
		{
			get { return _Depth; }
			set
			{
				_Depth = value;
				UseCustomDepth = true;
			}
		}

		/// <summary>
		/// If no AnimationId is set with the constructor it will not cycle, but if you change the speed it will take all the frames in the sprite.
		/// Else it will loop through the given animation.
		/// </summary>
		public string AnimationId;
		/// <summary>
		/// Index of the selected animation (which is different from the currently displayed frame)
		/// </summary>
		public uint AnimationIndex;
        /// <summary>
		/// Frames per step, use SetFramesPerSecond if you don't like doing the maths.
        /// </summary>
		public float AnimationSpeed;

		/// <summary>
		/// Used for determining when to increase the index and how many times;
		/// </summary>
		private float IndexIncrement;

		/// <summary>
		/// All parameters.
		/// </summary>
		public ManagedSprite(string targetSprite, PointF position, PointF scale, PointF origin, float rotation = 0, string animationId = "", uint startIndex = 0, float speed = 1, sbyte depth = 0)
		{
			TargetSprite = Resources.Sprites[targetSprite];
			Position = position;
			Scale = scale;
			Origin = origin;
			Rotation = rotation;
			AnimationId = animationId;
			AnimationIndex = startIndex;
			AnimationSpeed = speed;

			if (depth != 0) //prevent UseCustomDepth from being set
				Depth = depth;

			//add to resources for updates. weak so it will be removed when no longer used by user
			Resources.ManagedSprites.Add(new WeakReference(this));
		}
		/// <summary>
		/// No transformations initially set.
		/// </summary>
		public ManagedSprite(string targetSprite, string animationId = "", uint startIndex = 0, float speed = 1, sbyte depth = 0) : 
		this(targetSprite, new PointF(0, 0), new PointF(1, 1), new PointF(0, 0), 0, animationId, startIndex, speed, depth) { }
		/// <summary>
		/// With position, and no transformations.
		/// </summary>
		public ManagedSprite(string targetSprite, PointF position, string animationId = "", uint startIndex = 0, float speed = 1, sbyte depth = 0) :
			this(targetSprite, position, new PointF(1, 1), new PointF(0, 0), 0, animationId, startIndex, speed, depth) { }
		/// <summary>
		/// With position and scale.
		/// </summary>
		public ManagedSprite(string targetSprite, PointF position, PointF scale, string animationId = "", uint startIndex = 0, float speed = 1, sbyte depth = 0) :
			this(targetSprite, position, scale, new PointF(0, 0), 0, animationId, startIndex, speed, depth) { }

		public void Draw()
        {
			Draw(Position);
        }
		/// <summary>
		/// Override the position to draw at (does not update it)
		/// </summary>
		public void Draw(PointF pos)
		{
			uint frameIndex;
			if (AnimationId != "")
				frameIndex = TargetSprite.Animations[AnimationId][AnimationIndex];
			else
				frameIndex = AnimationIndex;

			if (UseCustomDepth)
				DrawX.Depth = _Depth;
			TargetSprite.Draw(pos, frameIndex, Scale, Origin, Rotation);
		}

		internal void Step()
        {
			IndexIncrement += AnimationSpeed;

			uint indexLength;
			if (AnimationId != "")
			{
				indexLength = (uint)(TargetSprite.Animations[AnimationId].Length - 1);
			}
			else
			{
				indexLength = (uint)(TargetSprite.IndexSize.Width * TargetSprite.IndexSize.Height);
			}

			while (IndexIncrement >= 1)
			{
				AnimationIndex++;

				//reset if over the length
				while (AnimationIndex > indexLength)
				{
					AnimationIndex %= indexLength;
				}

				IndexIncrement--;
			}
        }

        /// <summary>
        /// Sets speed per second instead of per step.
        /// </summary>
        /// <param name="fps">Amount of frames yer be wanting.</param>
        public void SetFramesPerSecond(float fps)
        {
			AnimationSpeed = 1 / (Game.Speed / fps);
        }
    }
}
