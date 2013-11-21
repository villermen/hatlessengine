using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Class that contains blueprints for sprites to be drawn in bulk.
    /// </summary>
    public class Spritemap
    {
        public string Id { get; private set; }

        public List<SpritemapBlueprint> Blueprints;

        internal Spritemap(string id, params SpritemapBlueprint[] blueprints)
        {
            Id = id;
            Blueprints = new List<SpritemapBlueprint>(blueprints);
        }

        /// <summary>
        /// Draw the entire Spritemap to a position.
        /// </summary>
        /// <param name="pos">Dnno, figure it out for yourself.</param>
        public void Draw(Position pos)
        {
            foreach (SpritemapBlueprint blueprint in Blueprints)
            {
                blueprint.Sprite.Draw(pos + blueprint.Rectangle.Position, blueprint.Frame);
            }
        }
    }
}
