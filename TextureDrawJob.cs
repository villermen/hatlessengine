using System;
using SDL2;
namespace HatlessEngine
{
	internal struct TextureDrawJob : IDrawJob
	{
		public DrawJobType Type { get; set; }
		public int Depth { get; set; }
		public SimpleRectangle Area { get; set; }

		public IntPtr Texture;
		public SimpleRectangle SourceRect;
		public Rectangle DestRect;

		/// <summary>
		/// Texture job.
		/// </summary>
		public TextureDrawJob(int depth, SimpleRectangle area, IntPtr texture, SimpleRectangle sourceRect, Rectangle destRect)
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