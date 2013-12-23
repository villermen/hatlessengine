using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

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

        internal Spritemap(string id, string filename)
        {
            Id = id;
            Blueprints = new List<SpritemapBlueprint>();

            BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            ushort protocolVersion = reader.ReadUInt16();
            switch (protocolVersion)
            {
                case 1:
                    ushort uniqueSprites = reader.ReadUInt16();
                    for (ushort i = 1; i <= uniqueSprites; i++)
                    {
                        Sprite currentSprite = Resources.Sprites[reader.ReadString()];
                        ushort spriteOccurrences = reader.ReadUInt16();
                        for (ushort j = 1; j <= spriteOccurrences; j++)
                        { 
						Blueprints.Add(new SpritemapBlueprint(currentSprite, new PointF(reader.ReadSingle(), reader.ReadSingle()), reader.ReadUInt32()));
                        }
                    }
                    break;
            }
            reader.Close();
        }

        /// <summary>
        /// Draw the entire Spritemap to a position.
        /// </summary>
        /// <param name="pos">Dnno, figure it out for yourself.</param>
		public void Draw(PointF pos)
        {
            foreach (SpritemapBlueprint blueprint in Blueprints)
            {
				blueprint.Sprite.Draw(new PointF(pos.X + blueprint.Rectangle.X, pos.Y + blueprint.Rectangle.Y), blueprint.Frame);
            }
        }

        public void WriteToFile(string filename)
        {
            //prepare data for writing
            Dictionary<string, List<Tuple<float, float, uint>>> data = new Dictionary<string, List<Tuple<float, float, uint>>>();
            foreach (SpritemapBlueprint blueprint in Blueprints)
            {
				Tuple<float, float, uint> tuple = new Tuple<float, float, uint>(blueprint.Rectangle.Location.X, blueprint.Rectangle.Location.Y, blueprint.Frame);

                if (!data.ContainsKey(blueprint.Sprite.Id))
                {
                    List<Tuple<float, float, uint>> list = new List<Tuple<float, float, uint>>();
                    list.Add(tuple);
                    data.Add(blueprint.Sprite.Id, list);
                }
                else
                    data[blueprint.Sprite.Id].Add(tuple);
            }

            //actually write
            BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write));
            writer.Write((ushort)1); //protocol version
            writer.Write((ushort)data.Count);
            foreach (KeyValuePair<string, List<Tuple<float, float, uint>>> uniqueSprite in data)
            {
                writer.Write(uniqueSprite.Key);
                writer.Write((ushort)uniqueSprite.Value.Count);
                foreach (Tuple<float, float, uint> tuple in uniqueSprite.Value)
                {
                    writer.Write(tuple.Item1);
                    writer.Write(tuple.Item2);
                    writer.Write(tuple.Item3);
                }
            }
            writer.Close();
        }
    }
}
