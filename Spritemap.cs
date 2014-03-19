using System;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
    /// <summary>
    /// Class that contains blueprints for sprites to be drawn in bulk.
    /// </summary>
    public class Spritemap
    {
        public string Id { get; private set; }

		public static readonly ushort ProtocolVersion = 1;

		public List<ManagedSprite> ManagedSprites;

		internal Spritemap(string id, params ManagedSprite[] managedSprites)
        {
            Id = id;
			ManagedSprites = new List<ManagedSprite>(managedSprites);
        }

        internal Spritemap(string id, string filename)
        {
            Id = id;
			ManagedSprites = new List<ManagedSprite>();

			BinaryReader reader = new BinaryReader(Resources.GetStream(filename));

			if (reader.ReadChars(4) != "HESm".ToCharArray())
				throw new ProtocolMismatchException("The file's magic number is not 'HESm' (HatlessEngine Spritemap)");

			//protocol version changes on updates to ManagedSprite
			if (reader.ReadUInt16() != ProtocolVersion)
				throw new ProtocolMismatchException("The file's protocol version is not equal to the required one (" + ProtocolVersion.ToString() + ")");

			ushort spriteCount = reader.ReadUInt16();

			for(ushort i = 0; i < spriteCount; i++)
			{
				string targetSprite = reader.ReadString();
				Point position = new Point(reader.ReadSingle(), reader.ReadSingle());
				Point scale = new Point(reader.ReadSingle(), reader.ReadSingle());
				Point origin = new Point(reader.ReadSingle(), reader.ReadSingle());
				float rotation = reader.ReadSingle();
				float rotationSpeed = reader.ReadSingle();
				string animationId = reader.ReadString();
				uint startIndex = reader.ReadUInt32();
				float animationSpeed = reader.ReadSingle();
				sbyte depth = reader.ReadSByte();
				
				ManagedSprites.Add(new ManagedSprite(targetSprite, position, scale, origin, rotation, rotationSpeed, animationId, startIndex, animationSpeed, depth));
			}

			reader.Close();            
        }

		/// <summary>
		/// Draw all the sprites in this spritemap at their absolute positions.
		/// </summary>
		public void Draw()
		{
			foreach(ManagedSprite sprite in ManagedSprites)
			{
				sprite.Draw();
			}
		}

        /// <summary>
		/// Draw all the sprites in this spritemap relative to a position.
        /// </summary>
		public void Draw(Point pos)
        {
			foreach (ManagedSprite sprite in ManagedSprites)
            {
				sprite.Draw(new Point(pos.X + sprite.Position.X, pos.Y + sprite.Position.Y));
            }
        }

		/// <summary>
		/// Writes current state of all the ManagedSprites to a file.
		/// Current state meaning it will not use the values the sprites have been created with.
		/// If this behavior is undesired use FreezeSprites immediately after creation to keep them in their default position.
		/// </summary>
		public void WriteToFile(string filename)
        {
			BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
			writer.Write("HESm".ToCharArray());
			writer.Write(ProtocolVersion);
			writer.Write((ushort)ManagedSprites.Count);
			foreach (ManagedSprite sprite in ManagedSprites)
            {
				//writewritewritewrite...
				writer.Write(sprite.TargetSprite.Id);
				writer.Write(sprite.Position.X);
				writer.Write(sprite.Position.Y);
				writer.Write(sprite.Scale.X);
				writer.Write(sprite.Scale.Y);
				writer.Write(sprite.Origin.X);
				writer.Write(sprite.Origin.Y);
				writer.Write(sprite.Rotation);
				writer.Write(sprite.RotationSpeed);
				writer.Write(sprite.AnimationId);
				writer.Write(sprite.AnimationIndex);
				writer.Write(sprite.AnimationSpeed);
				writer.Write(sprite.Depth);
            }
            writer.Close();
        }

		/// <summary>
		/// Will freeze all sprites in this spritemap, setting their PerformStep to false.
		/// </summary>
		public void FreezeSprites()
		{
			foreach(ManagedSprite sprite in ManagedSprites)
				sprite.PerformStep = false;
		}

		/// <summary>
		/// Will unfreeze all sprites in this spritemap, setting their PerformStep to true.
		/// </summary>
		public void UnfreezeSprites()
		{
			foreach(ManagedSprite sprite in ManagedSprites)
				sprite.PerformStep = true;
		}
    }
}
