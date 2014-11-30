using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Used by Game to quickly discover what objects an object could potentially interact with.
	/// </summary>
	internal class QuadTree
	{
		private static byte MaxObjects = 5;
		//private static byte MaxLevels = 5;

		private byte Level;
		private Rectangle Bounds;
		private Point Center;
		private Point ChildSize;
		private QuadTree[] Children = new QuadTree[4];

		private List<PhysicalObject> Objects = new List<PhysicalObject>();

		/// <summary>
		/// The mother of all quadtrees.
		/// </summary>
		public QuadTree(Rectangle bounds)
			: this(0, bounds, Resources.PhysicalObjects) { }

		/// <summary>
		/// A teensy quadtree baby.
		/// </summary>
		private QuadTree(byte level, Rectangle bounds, List<PhysicalObject> objects)
		{
			Level = level;
			Bounds = bounds;
			Center = bounds.Position + bounds.Size / 2f;
			ChildSize = bounds.Size / 2f;

			if (objects.Count > MaxObjects)
			{
				//decide in what childtree an object would fit and add it there
				List<PhysicalObject> ChildObjects0 = new List<PhysicalObject>();
				List<PhysicalObject> ChildObjects1 = new List<PhysicalObject>();
				List<PhysicalObject> ChildObjects2 = new List<PhysicalObject>();
				List<PhysicalObject> ChildObjects3 = new List<PhysicalObject>();

				foreach (PhysicalObject obj in objects)
				{
					bool[] fits = FitObject(obj);
					if (!fits[4])
					{
						if (fits[0])
							ChildObjects0.Add(obj);
						else if (fits[1])
							ChildObjects1.Add(obj);
						else if (fits[2])
							ChildObjects2.Add(obj);
						else
							ChildObjects3.Add(obj);
					}
					else
						Objects.Add(obj);
				}

				//create subtrees and add everything that fits inside of em
				Children[0] = new QuadTree((byte)(Level + 1), new Rectangle(Bounds.Position, ChildSize), ChildObjects0);
				Children[1] = new QuadTree((byte)(Level + 1), new Rectangle(new Point(Center.X, Bounds.Position.Y), ChildSize), ChildObjects1);
				Children[2] = new QuadTree((byte)(Level + 1), new Rectangle(new Point(Bounds.Position.X, Center.Y), ChildSize), ChildObjects2);
				Children[3] = new QuadTree((byte)(Level + 1), new Rectangle(Center, ChildSize), ChildObjects3);
			}
			else
				Objects = objects;
		}

		public List<PhysicalObject> GetCollisionCandidates(PhysicalObject obj)
		{
			List<PhysicalObject> targets = new List<PhysicalObject>(Objects);

			//check in the child trees this object overlaps with
			if (Children[0] != null)
			{
				bool[] fits = FitObject(obj);
				if (fits[0])
					targets.AddRange(Children[0].GetCollisionCandidates(obj));
				if (fits[1])
					targets.AddRange(Children[1].GetCollisionCandidates(obj));
				if (fits[2])
					targets.AddRange(Children[2].GetCollisionCandidates(obj));
				if (fits[3])
					targets.AddRange(Children[3].GetCollisionCandidates(obj));
			}

			targets.Remove(obj);

			return targets;
		}

		/// <summary>
		/// Returns the childtrees the given object fits in.
		/// 0-3, left-to-right, top-to-bottom.
		/// 4 is true when the object doesn't fit in just one quadrant.
		/// </summary>
		private bool[] FitObject(PhysicalObject obj)
		{
			bool[] fits = new bool[5];
			byte quadrants = 0;

			if (obj.CoverableArea.Position.X <= Center.X)
			{
				if (obj.CoverableArea.Position.Y <= Center.Y)
				{
					fits[0] = true;
					quadrants++;
				}
				if (obj.CoverableArea.Position2.Y >= Center.Y)
				{
					fits[2] = true;
					quadrants++;
				}
			}
			if (obj.CoverableArea.Position2.X >= Center.X)
			{
				if (obj.CoverableArea.Position.Y <= Center.Y)
				{
					fits[1] = true;
					quadrants++;
				}
				if (obj.CoverableArea.Position2.Y >= Center.Y)
				{
					fits[3] = true;
					quadrants++;
				}
			}

			if (quadrants > 1)
				fits[4] = true;

			return fits;
		}

		/// <summary>
		/// For debugging purposes.
		/// </summary>
		public void Draw()
		{
			/*DrawSettings.RectangleBounds(Bounds, new Color(Level));
			if (Children[0] != null)
			{
				Children[0].Draw();
				Children[1].Draw();
				Children[2].Draw();
				Children[3].Draw();
			}*/
		}
	}
}