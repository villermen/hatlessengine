using System;
using System.Drawing;

namespace HatlessEngine
{
    /// <summary>
    /// Contains information about a sprite used and referenced in Spritemap.
    /// </summary>
    public class SpritemapBlueprint
    {
        public Sprite Sprite;
        public uint Frame;
		public RectangleF Rectangle;

        /// <summary>
        /// Define the sprite by spriteId, skipping the need for Resources.Sprites[]
        /// </summary>
        /// <param name="spriteId">Id of the sprite</param>
        /// <param name="pos">Position to display the sprite at (relative to Spritemap's draw position)</param>
        /// <param name="frame">Frame of the sprite to display.</param>
		public SpritemapBlueprint(string spriteId, PointF pos, uint frame = 0)
            : this(Resources.Sprites[spriteId], pos, frame) { }

        /// <summary>
        /// Define the sprite.
        /// </summary>
        /// <param name="sprite">The sprite</param>
        /// <param name="pos">Position to display the sprite at (relative to Spritemap's draw position)</param>
        /// <param name="frame">Frame of the sprite to display.</param>
		public SpritemapBlueprint(Sprite sprite, PointF pos, uint frame = 0)
        {
            Sprite = sprite;
            Frame = frame;
			Rectangle = new RectangleF(pos, Sprite.FrameSize);
        }
    }
}
