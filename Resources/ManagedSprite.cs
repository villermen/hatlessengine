using System;

namespace HatlessEngine
{
	/// <summary>
	/// Holds parameters for drawing a sprite so you don't have to store those for yourself for the sprite.
	/// See it as an instance of a sprite that can be drawn.
	/// </summary>
	public class ManagedSprite : ComplexRectangle
	{
		public Sprite TargetSprite { get; private set; }

		public int Depth;

		private string _AnimationID;

		/// <summary>
		/// The animation index of the sprite, if none is given it will loop through all frames.
		/// </summary>
		public string AnimationID
		{
			get { return _AnimationID; }
			set
			{
				if (value == "")
				{
					_AnimationID = "";
					AnimationLength = (int)(TargetSprite.IndexSize.X * TargetSprite.IndexSize.Y);
				}
				else if (TargetSprite.Animations.ContainsKey(value))
				{
					_AnimationID = value;
					AnimationLength = TargetSprite.Animations[value].Length;
				}
				else
					throw new IndexNotFoundException("TargetSprite doesn't have an animation with index '" + value + "'.");
			}
		}

		/// <summary>
		/// Index of the selected animation (which can be different from the currently displayed frame if an AnimationID is set).
		/// </summary>
		public int AnimationIndex;

		private int AnimationLength;

		/// <summary>
		/// Frames per step, use SetFramesPerSecond if you want it to be independent of game speed or just don't feel like doing maths.
		/// </summary>
		public float AnimationSpeed;

		/// <summary>
		/// Used for determining when to increase the index and how many times.
		/// </summary>
		private float IndexIncrement;

		/// <summary>
		/// While this is false, the sprite will not change in any way.
		/// </summary>
		public bool PerformStep = true;

		/// <summary>
		/// All parameters.
		/// </summary>
		public ManagedSprite(string targetSpriteID, Point pos, string animationID = "", int startIndex = 0, float animationSpeed = 1f, int depth = 0)
		{
			TargetSprite = Resources.Sprites[targetSpriteID];
			AnimationID = animationID;
			AnimationIndex = startIndex;
			AnimationSpeed = animationSpeed;
			Depth = depth;

			Position = pos;
			Size = TargetSprite.FrameSize;

			//Add to resources for updates. Weak so it will be removed when no longer used by user.
			Resources.ManagedSprites.Add(new WeakReference(this));
		}

		public void Draw()
		{
			if (_AnimationID == "")
				TargetSprite.Draw(this, AnimationIndex, Depth);
			else //resolve AnimationID to actual frame
				TargetSprite.Draw(this, TargetSprite.Animations[_AnimationID][AnimationIndex], Depth);
		}

		internal void Step()
		{
			if (PerformStep)
			{
				//animation
				IndexIncrement += AnimationSpeed;

				while (IndexIncrement >= 1)
				{
					AnimationIndex++;

					//reset if over the length
					if (AnimationIndex < 0 || AnimationIndex > AnimationLength)
						AnimationIndex = Misc.Modulus(AnimationIndex, AnimationLength);

					IndexIncrement--;
				}
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
	}
}