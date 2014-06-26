using System;
using System.Collections.Generic;

namespace HatlessEngine
{
	public enum CollisionRuleType
	{
		Shape = 0,
		Object = 1,
		ObjectType = 2,
		Objectmap = 3,
		ManagedSprite = 4,
		Spritemap = 5,
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

	public class CollisionRule
	{
		public bool Active = true;
		internal CollisionRuleType Type;
		internal CollisionAction Action;
		internal Delegate Method;
		internal bool DeactivateAfterCollision = false;

		internal object Target;
		internal Point SpritemapOffset;
		internal bool FilterEnabled;
		internal List<Sprite> SpritemapFilter;
		internal List<Type> ObjectmapFilter;

		public CollisionRule(IShape shape, CollisionAction action, Action<IShape> method = null)
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
				throw new InvalidObjectTypeException("objType (" + objType.ToString() + ") is not a subclass of PhysicalObject");

			Type = CollisionRuleType.ObjectType;
			Target = objType;
			Action = action;
			Method = method;
		}
		public CollisionRule(Objectmap objMap, CollisionAction action, Type[] objFilter = null, Action<PhysicalObject> method = null)
		{
			//validate objFilter
			foreach(Type objType in objFilter)
			{
				if (!objType.IsSubclassOf(typeof(PhysicalObject)))
					throw new InvalidObjectTypeException("objType in objFilter (" + objType.ToString() + ") is not a subclass of PhysicalObject");
			}

			Type = CollisionRuleType.Objectmap;
			Target = objMap;
			if (objFilter != null)
			{
				ObjectmapFilter = new List<Type>(objFilter);
				FilterEnabled = true;
			}
			else
				FilterEnabled = false;
			Action = action;
			Method = method;
		}
		//objectmapid
		public CollisionRule(ManagedSprite mSprite, CollisionAction action, Action<ManagedSprite> method = null)
		{
			Type = CollisionRuleType.ManagedSprite;
			Target = mSprite;
			Action = action;
			Method = method;
		}
		public CollisionRule(Spritemap spritemap, Point spritemapOffset, CollisionAction action, Sprite[] spriteFilter = null, Action<PhysicalObject> method = null)
		{
			Type = CollisionRuleType.Spritemap;
			Target = spritemap;
			SpritemapOffset = spritemapOffset;
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
		public CollisionRule(string spritemapID, Point spritemapOffset, CollisionAction action, string[] spriteIDFilter = null, Action<PhysicalObject> method = null)
			: this(Resources.Spritemaps[spritemapID], spritemapOffset, action, ProcessSpriteIDs(spriteIDFilter), method) { }

		internal void CallMethod(object result)
		{
			if (Method != null)
				Method.DynamicInvoke(result);
		}

		private static Sprite[] ProcessSpriteIDs(string[] spriteIDs)
		{
			if (spriteIDs == null)
				return null;
			else
			{
				Sprite[] sprites = new Sprite[spriteIDs.Length];
				for (int i = 0; i < spriteIDs.Length; i++)
				{
					sprites[i] = Resources.Sprites[spriteIDs[i]];
				}
				return sprites;
			}
		}
	}
}