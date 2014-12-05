using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	public sealed class CollisionRule
	{
		public bool Active = true;
		internal CollisionRuleType Type;
		internal CollisionAction Action;
		internal Delegate Method;
		internal bool DeactivateAfterCollision = false;

		internal object Target;
		internal bool FilterEnabled;
		internal List<Sprite> SpritemapFilter;
		internal List<Type> ObjectmapFilter;

		#region Constructors

		public CollisionRule(Shape shape, CollisionAction action, Action<Shape> method = null)
		{
			Type = CollisionRuleType.Shape;
			Target = shape;
			Action = action;
			Method = method;
		}
		public CollisionRule(PhysicalObject obj, CollisionAction action, Action<PhysicalObject> method = null)
		{
			Type = CollisionRuleType.Object;
			Target = obj;
			Action = action;
			Method = method;
		}
		public CollisionRule(Type objType, CollisionAction action, Action<PhysicalObject> method = null)
		{
			//validate objType
			if (!objType.IsSubclassOf(typeof(PhysicalObject)))
				throw new InvalidObjectTypeException("objType (" + objType + ") is not a subclass of PhysicalObject");

			Type = CollisionRuleType.ObjectType;
			Target = objType;
			Action = action;
			Method = method;
		}
		public CollisionRule(Objectmap objMap, CollisionAction action, Type[] objFilter = null, Action<PhysicalObject> method = null)
		{
			Type = CollisionRuleType.Objectmap;
			Target = objMap;
			Action = action;
			Method = method;

			if (objFilter != null)
			{
				//validate the filter
				foreach (Type objType in objFilter)
				{
					if (!objType.IsSubclassOf(typeof (PhysicalObject)))
						throw new InvalidObjectTypeException("objType in objFilter (" + objType +
						                                     ") is not a subclass of PhysicalObject");
				}

				ObjectmapFilter = new List<Type>(objFilter);

				FilterEnabled = true;
			}
			else
				FilterEnabled = false;
		}
		public CollisionRule(Spritemap spritemap, CollisionAction action, IEnumerable<Sprite> spriteFilter = null, Action<PhysicalObject> method = null)
		{
			Type = CollisionRuleType.Spritemap;
			Target = spritemap;
			if (spriteFilter != null)
			{
				SpritemapFilter = new List<Sprite>(spriteFilter);
				FilterEnabled = true;
			}
			else
				FilterEnabled = false;
			Action = action;
			Method = method;
		}
		public CollisionRule(string spritemapId, CollisionAction action, IList<string> spriteIdFilter = null, Action<PhysicalObject> method = null)
			: this(Resources.Spritemaps[spritemapId], action, ProcessSpriteIDs(spriteIdFilter), method) { }

		private static IEnumerable<Sprite> ProcessSpriteIDs(IList<string> spriteIDs)
		{
			if (spriteIDs == null)
				return null;
			
			Sprite[] sprites = new Sprite[spriteIDs.Count];
			for (int i = 0; i < spriteIDs.Count; i++)
			{
				sprites[i] = Resources.Sprites[spriteIDs[i]];
			}
			return sprites;
		}

		#endregion

		internal void CallMethod(object result)
		{
			if (Method != null)
				Method.DynamicInvoke(result);
		}
	}

	public enum CollisionRuleType
	{
		Shape = 0,
		Object = 1,
		ObjectType = 2,
		Objectmap = 3,
		Spritemap = 4,
		//Shapemap = 6
	}

	public enum CollisionAction
	{
		/// <summary>
		/// Do nothing. For triggering a method only.
		/// </summary>
		None = 0,
		/// <summary>
		/// Set object's speed to 0 at touching point.
		/// </summary>
		Block = 1,
		/// <summary>
		/// Bounce perfectly off the surface changing the SpeedDirection only.
		/// </summary>
		Bounce = 2,
		//Slide = 4, keep distance left or only distance on unlocked axis left?			
	}
}