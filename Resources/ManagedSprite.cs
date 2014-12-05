using System;
using System.Collections.Generic;

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

		private string _animationId;

		/// <summary>
		/// The animation index of the sprite, if none is given it will loop through all frames.
		/// </summary>
		public string AnimationId
		{
			get { return _animationId; }
			set
			{
				if (value == "")
				{
					_animationId = "";
					_animationLength = (int)(TargetSprite.IndexSize.X * TargetSprite.IndexSize.Y);
				}
				else if (TargetSprite.Animations.ContainsKey(value))
				{
					_animationId = value;
					_animationLength = TargetSprite.Animations[value].Length;
				}
				else
					throw new KeyNotFoundException("TargetSprite doesn't have an animation with index '" + value + "'.");
			}
		}

		/// <summary>
		/// Index of the selected animation (which can be different from the currently displayed frame if an AnimationID is set).
		/// </summary>
		public int AnimationIndex;

		private int _animationLength;

		/// <summary>
		/// Frames per step, use SetFramesPerSecond if you want it to be independent of game speed or just don't feel like doing maths.
		/// </summary>
		public float AnimationSpeed;

		/// <summary>
		/// Used for determining when to increase the index and how many times.
		/// </summary>
		private float _indexIncrement;

		/// <summary>
		/// While this is false, the sprite will not change in any way.
		/// </summary>
		public bool PerformStep = true;

		/// <summary>
		/// All parameters.
		/// </summary>
		public ManagedSprite(Sprite targetSprite, Point pos, string animationId = "", int startIndex = 0, float animationSpeed = 1f, int depth = 0)
		{
			TargetSprite = targetSprite;
			AnimationId = animationId;
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
			TargetSprite.Draw(this, 
				_animationId == "" ? AnimationIndex : TargetSprite.Animations[_animationId][AnimationIndex],
				Depth);
		}

		internal void Step()
		{
			if (PerformStep)
			{
				//animation
				_indexIncrement += AnimationSpeed;

				while (_indexIncrement >= 1)
				{
					AnimationIndex++;

					//reset if over the length
					if (AnimationIndex < 0 || AnimationIndex > _animationLength)
						AnimationIndex = Misc.Modulus(AnimationIndex, _animationLength);

					_indexIncrement--;
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