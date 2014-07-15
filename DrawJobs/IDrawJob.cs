using System;

namespace HatlessEngine
{
	internal interface IDrawJob
	{
		DrawJobType Type { get; set; }
		int Depth { get; set; }
		Rectangle Area { get; set; }
	}

	internal enum DrawJobType 
	{ 
		Texture = 0, 
		Lines = 1 
	}
}