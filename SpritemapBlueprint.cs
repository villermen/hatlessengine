using System;

namespace HatlessEngine
{
    /// <summary>
    /// Contains information about a sprite used and referenced in Spritemap.
    /// </summary>
    public class SpritemapBlueprint
    {
        public Sprite Sprite;
        public uint Frame;
        public Rectangle Rectangle;

        /// <summary>
        /// Define the sprite.
        /// </summary>
        /// <param name="spriteId">Id of the sprite...</param>
        /// <param name="pos">Position to display the sprite at (relative to Spritemap's draw position)</param>
        /// <param name="frame">Frame of the sprite to display.</param>
        public SpritemapBlueprint(string spriteId, Position pos, uint frame = 0)
        {
            Sprite = Resources.Sprites[spriteId];
            Frame = frame;
            Rectangle = new Rectangle(pos, Sprite.Size);
        }
    }
}
