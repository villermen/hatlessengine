using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
	/// <summary>
	/// Class that contains blueprints for sprites to be drawn in bulk.
	/// </summary>
	public sealed class Spritemap : IResource
	{
		public string Id { get; private set; }

		public static readonly ushort ProtocolVersion = 1;

		public List<ManagedSprite> ManagedSprites;

		private Spritemap(string id)
		{
			Id = id;
			Resources.Spritemaps.Add(Id, this);
		}

		public Spritemap(string id, params ManagedSprite[] managedSprites)
			: this(id)
		{
			ManagedSprites = new List<ManagedSprite>(managedSprites);
		}

		public Spritemap(string id, string filename)
			: this(id)
		{
			ManagedSprites = new List<ManagedSprite>();

			BinaryReader reader = Resources.GetStream(filename);

			if (new string(reader.ReadChars(4)) != "HESm")
				throw new ProtocolMismatchException("The file's magic number is not 'HESm' (HatlessEngine Spritemap)");
				
			if (reader.ReadUInt16() != ProtocolVersion)
				throw new ProtocolMismatchException("The file's protocol version is not equal to the required one (" + ProtocolVersion + ")");

			ushort spriteCount = reader.ReadUInt16();

			for(ushort i = 0; i < spriteCount; i++)
			{
				string targetSprite = reader.ReadString();
				Point position = new Point(reader.ReadSingle(), reader.ReadSingle());
				Point size = new Point(reader.ReadSingle(), reader.ReadSingle());
				Point origin = new Point(reader.ReadSingle(), reader.ReadSingle());
				float rotation = reader.ReadSingle();
				string animationId = reader.ReadString();
				int startIndex = reader.ReadInt32();
				float animationSpeed = reader.ReadSingle();
				sbyte depth = reader.ReadSByte();
				
				ManagedSprite mSprite = new ManagedSprite(targetSprite, position, animationId, startIndex, animationSpeed, depth)
				{
					Size = size,
					Origin = origin,
					Rotation = rotation
				};

				ManagedSprites.Add(mSprite);
			}

			reader.Close();			
		}

		/// <summary>
		/// Draw all the sprites in this spritemap.
		/// </summary>
		public void Draw()
		{
			foreach(ManagedSprite sprite in ManagedSprites)
			{
				sprite.Draw();
			}
		}

		/// <summary>
		/// Writes current state of all the ManagedSprites to a file.
		/// Current state meaning it will not use the values the sprites have been created with.
		/// If this behavior is undesired use FreezeSprites immediately after creation to keep them in their default position.
		/// </summary>
		public void WriteToFile(string filename)
		{
			BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Truncate, FileAccess.Write, FileShare.None));
			writer.Write("HESm".ToCharArray());
			writer.Write(ProtocolVersion);
			writer.Write((ushort)ManagedSprites.Count);
			foreach (ManagedSprite sprite in ManagedSprites)
			{
				//writewritewritewrite...
				writer.Write(sprite.TargetSprite.Id);
				writer.Write(sprite.Position.X);
				writer.Write(sprite.Position.Y);
				writer.Write(sprite.Size.X);
				writer.Write(sprite.Size.Y);
				writer.Write(sprite.Origin.X);
				writer.Write(sprite.Origin.Y);
				writer.Write(sprite.Rotation);
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

		public void Destroy()
		{
			Resources.Spritemaps.Remove(Id);
		}

		public static implicit operator Spritemap(string str)
		{
			return Resources.Spritemaps[str];
		}
	}
}