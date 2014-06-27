using System;

namespace HatlessEngine
{
	/// <summary>
	/// Holds parameters for drawing a sprite so you don't have to store those for yourself for the sprite.
	/// See it as an instance of a sprite that can be drawn.
	/// </summary>
	public sealed class ManagedSprite
	{
		public Sprite TargetSprite { get; private set; }

		public Point Position;

		public Point Scale;

		public Point Origin;
		public float Rotation;
		public float RotationSpeed;

		public int Depth;

		/// <summary>
		/// If no AnimationID is set with the constructor it will not cycle, but if you change the AnimationSpeed it will take all the frames in the sprite.
		/// Else it will loop through the given animation.
		/// </summary>
		public string AnimationID;
		/// <summary>
		/// Index of the selected animation (which is different from the currently displayed frame)
		/// </summary>
		public int AnimationIndex;
		/// <summary>
		/// Frames per step, use SetFramesPerSecond if you don't like doing the maths.
		/// </summary>
		public float AnimationSpeed;

		/// <summary>
		/// Used for determining when to increase the index and how many times;
		/// </summary>
		private float IndexIncrement;

		/// <summary>
		/// While this is false, the sprite will not change in any way. (animation, rotation)
		/// </summary>
		public bool PerformStep = true;

		public Rectangle SpriteRectangle 
		{
			get { return new Rectangle(Position, Scale * TargetSprite.FrameSize, Origin, Rotation); }
			set
			{
				Position = value.Position;
				Scale = value.Size / TargetSprite.FrameSize;
				Origin = value.Origin;
				Rotation = value.Rotation;
			}
		}

		/// <summary>
		/// All parameters.
		/// </summary>
		public ManagedSprite(string targetSprite, Point position, Point scale, Point origin, float rotation = 0f, float rotationSpeed = 0f, string animationID = "", int startIndex = 0, float animationSpeed = 1f, int depth = 0)
		{
			TargetSprite = Resources.Sprites[targetSprite];
			Position = position;
			Scale = scale;
			Origin = origin;
			Rotation = rotation;
			RotationSpeed = rotationSpeed;
			AnimationID = animationID;
			AnimationIndex = startIndex;
			AnimationSpeed = animationSpeed;

			Depth = depth;

			//add to resources for updates. weak so it will be removed when no longer used by user
			Resources.ManagedSprites.Add(new WeakReference(this));
		}
		/// <summary>
		/// No transformations initially set.
		/// </summary>
		public ManagedSprite(string targetSprite, string animationID = "", int startIndex = 0, float animationSpeed = 1f, int depth = 0) : 
		this(targetSprite, new Point(0f, 0f), new Point(1f, 1f), new Point(0, 0), 0f, 0f, animationID, startIndex, animationSpeed, depth) { }
		/// <summary>
		/// With position, and no transformations.
		/// </summary>
		public ManagedSprite(string targetSprite, Point position, string animationID = "", int startIndex = 0, float animationSpeed = 1f, int depth = 0) :
		this(targetSprite, position, new Point(1f, 1f), new Point(0, 0), 0f, 0f, animationID, startIndex, animationSpeed, depth) { }
		/// <summary>
		/// With position and scale.
		/// </summary>
		public ManagedSprite(string targetSprite, Point position, Point scale, string animationID = "", int startIndex = 0, float animationSpeed = 1, int depth = 0) :
		this(targetSprite, position, scale, new Point(0f, 0f), 0f, 0f, animationID, startIndex, animationSpeed, depth) { }

		public void Draw()
		{
			Draw(Position);
		}
		/// <summary>
		/// Override the position to draw at (does not update it)
		/// </summary>
		public void Draw(Point pos)
		{
			int frameIndex;
			if (AnimationID != "")
				frameIndex = TargetSprite.Animations[AnimationID][AnimationIndex];
			else
				frameIndex = AnimationIndex;

			TargetSprite.Draw(pos, Scale, Origin, frameIndex, Rotation, Depth);
		}

		internal void Step()
		{
			if (PerformStep)
			{
				//animation
				IndexIncrement += AnimationSpeed;

				int indexLength;
				if (AnimationID != "")
				{
					indexLength = TargetSprite.Animations[AnimationID].Length - 1;
				}
				else
				{
					indexLength = (int)(TargetSprite.IndexSize.X * TargetSprite.IndexSize.Y);
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

				//rotation
				Rotation += RotationSpeed;
			}
		}

		/// <summary>
		/// Set AnimationSpeed per second instead of per step.
		/// </summary>
		/// <param name="fps">Amount of frames yer be wanting.</param>
		public void SetFramesPerSecond(float fps)
		{
			AnimationSpeed = 1 / (Game.StepsPerSecond / fps);
		}

		/// <summary>
		/// Set RotationSpeed per second instead of per step.
		/// </summary>
		/// <param name="degrees">Degrees per second.</param>
		public void SetRotationDegreesPerSecond(float degrees)
		{
			RotationSpeed = 1 / (Game.StepsPerSecond / degrees);
		}
	}
}