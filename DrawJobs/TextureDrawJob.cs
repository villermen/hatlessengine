using System;
using SDL2;

namespace HatlessEngine
{
	internal struct TextureDrawJob : IDrawJob
	{
		public DrawJobType Type { get; set; }
		public int Depth { get; set; }
		public Rectangle Area { get; set; }

		public IntPtr Texture;
		public Rectangle SourceRect;
		public ComplexRectangle DestRect;

		/// <summary>
		/// Texture job.
		/// </summary>
		public TextureDrawJob(int depth, Rectangle area, IntPtr texture, Rectangle sourceRect, ComplexRectangle destRect)
			: this()
		{
			Texture = texture;
			SourceRect = sourceRect;
			DestRect = destRect;

			Type = DrawJobType.Texture;
			Depth = depth;
			Area = area;
		}
	}
}