using System.Collections.Generic;

namespace HatlessEngine
{
	/// <summary>
	/// Used by Game to quickly discover what objects an object could potentially interact with.
	/// </summary>
	internal class QuadTree
	{
		private const byte MaxObjects = 5;
		//private static byte MaxLevels = 5;

		private Point _center;
		private readonly QuadTree[] _children = new QuadTree[4];
		private readonly List<PhysicalObject> _objects = new List<PhysicalObject>();

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
			byte level1 = level;
			Rectangle bounds1 = bounds;
			_center = bounds.Position + bounds.Size / 2f;
			Point childSize = bounds.Size / 2f;

			if (objects.Count > MaxObjects)
			{
				//decide in what childtree an object would fit and add it there
				List<PhysicalObject> childObjects0 = new List<PhysicalObject>();
				List<PhysicalObject> childObjects1 = new List<PhysicalObject>();
				List<PhysicalObject> childObjects2 = new List<PhysicalObject>();
				List<PhysicalObject> childObjects3 = new List<PhysicalObject>();

				foreach (PhysicalObject obj in objects)
				{
					bool[] fits = FitObject(obj);
					if (!fits[4])
					{
						if (fits[0])
							childObjects0.Add(obj);
						else if (fits[1])
							childObjects1.Add(obj);
						else if (fits[2])
							childObjects2.Add(obj);
						else
							childObjects3.Add(obj);
					}
					else
						_objects.Add(obj);
				}

				//create subtrees and add everything that fits inside of em
				_children[0] = new QuadTree((byte)(level1 + 1), new Rectangle(bounds1.Position, childSize), childObjects0);
				_children[1] = new QuadTree((byte)(level1 + 1), new Rectangle(new Point(_center.X, bounds1.Position.Y), childSize), childObjects1);
				_children[2] = new QuadTree((byte)(level1 + 1), new Rectangle(new Point(bounds1.Position.X, _center.Y), childSize), childObjects2);
				_children[3] = new QuadTree((byte)(level1 + 1), new Rectangle(_center, childSize), childObjects3);
			}
			else
				_objects = objects;
		}

		public List<PhysicalObject> GetCollisionCandidates(PhysicalObject obj)
		{
			List<PhysicalObject> targets = new List<PhysicalObject>(_objects);

			//check in the child trees this object overlaps with
			if (_children[0] != null)
			{
				bool[] fits = FitObject(obj);
				if (fits[0])
					targets.AddRange(_children[0].GetCollisionCandidates(obj));
				if (fits[1])
					targets.AddRange(_children[1].GetCollisionCandidates(obj));
				if (fits[2])
					targets.AddRange(_children[2].GetCollisionCandidates(obj));
				if (fits[3])
					targets.AddRange(_children[3].GetCollisionCandidates(obj));
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

			if (obj.CoverableArea.Position.X <= _center.X)
			{
				if (obj.CoverableArea.Position.Y <= _center.Y)
				{
					fits[0] = true;
					quadrants++;
				}
				if (obj.CoverableArea.Position2.Y >= _center.Y)
				{
					fits[2] = true;
					quadrants++;
				}
			}
			if (obj.CoverableArea.Position2.X >= _center.X)
			{
				if (obj.CoverableArea.Position.Y <= _center.Y)
				{
					fits[1] = true;
					quadrants++;
				}
				if (obj.CoverableArea.Position2.Y >= _center.Y)
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