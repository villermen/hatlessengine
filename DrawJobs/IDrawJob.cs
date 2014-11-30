namespace HatlessEngine
{
	internal interface IDrawJob
	{
		int Depth { get; set; }
		Rectangle Area { get; set; }
	}
}