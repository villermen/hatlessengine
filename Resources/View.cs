using System;

namespace HatlessEngine
{
	[Serializable]
	public class View : IResource
	{
		public string ID { get; private set; }
		public Rectangle Area;
		public Rectangle Viewport;
		public bool Active;
		public bool AreaSizeIsViewportSize;

		public View(string id, Rectangle area, Rectangle viewport, bool areaSizeIsViewportSize = false, bool active = true)
		{
			//validate if viewport is entirely within the window
			if (viewport.Position.X < 0f || viewport.Position.Y < 0f || viewport.Position2.X > 1f || viewport.Position2.Y > 1f)
				throw new ArgumentOutOfRangeException("viewport", "viewport lies (partially) outside of the window area. Use fractional values (0f-1f) to specify the part of the window it should project to.");

			ID = id;
			Area = area;
			Viewport = viewport;
			AreaSizeIsViewportSize = areaSizeIsViewportSize;
			Active = active;

			Resources.Views.Add(ID, this);
		}

		public void Destroy()
		{
			Resources.Views.Remove(ID);
		}
	}
}