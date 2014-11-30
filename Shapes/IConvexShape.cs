namespace HatlessEngine
{
	/// <summary>
	/// Interface for the bare necessities of SAT-checking.
	/// </summary>
	public interface IConvexShape
	{
		Point[] GetPoints();
		Point[] GetPerpAxes();
		bool IntersectsWith(IConvexShape shape);
	}
}
