using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

namespace HatlessEngine
{
	/// <summary>
	/// Stores a list of objects, that can be mass-created and removed.
	/// Convenient way of creating levels or maps in general.
	/// </summary>
	public class Objectmap : IResource
	{
		public string ID { get; private set; }

		/// <summary>
		/// protocol version changes on changes to ObjectBlueprint.
		/// </summary>
		public static readonly ushort ProtocolVersion = 1;

		public List<ObjectBlueprint> Blueprints;
		public List<GameObject> ActiveObjects = new List<GameObject>();
		internal List<PhysicalObject> ActivePhysicalObjects = new List<PhysicalObject>();

		private Objectmap(string id)
		{
			ID = id;
			Resources.Objectmaps.Add(ID, this);
		}

		public Objectmap(string id, params ObjectBlueprint[] blueprints)
			: this(id)
		{
			Blueprints = new List<ObjectBlueprint>(blueprints);
		}
			
		public Objectmap(string id, string filename)
			: this(id)
		{
			BinaryReader reader = Resources.GetStream(filename);

			if (new String(reader.ReadChars(4)) != "HEOm")
				throw new ProtocolMismatchException("The file's magic number is not 'HEOm' (HatlessEngine Objectmap)");

			if (reader.ReadUInt16() != ProtocolVersion)
				throw new ProtocolMismatchException("The file's protocol version is not equal to the required one (" + ProtocolVersion.ToString() + ")");
				
			BinaryFormatter formatter = new BinaryFormatter();
			Blueprints = (List<ObjectBlueprint>)formatter.Deserialize(reader.BaseStream);

			reader.Close();
		}

		public List<GameObject> CreateObjects()
		{
			List<GameObject> returnList = new List<GameObject>();
			foreach (ObjectBlueprint blueprint in Blueprints)
			{
				GameObject logicalObject = (GameObject)Activator.CreateInstance(blueprint.Type, blueprint.Arguments);
				ActiveObjects.Add(logicalObject);
				//for collision checking against this map
				if (blueprint.Type.IsSubclassOf(typeof(PhysicalObject)))
					ActivePhysicalObjects.Add((PhysicalObject)logicalObject);
				returnList.Add(logicalObject);
			}
			return returnList;
		}

		public void WriteToFile(string filename)
		{
			BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Truncate, FileAccess.Write, FileShare.None));
			writer.Write("HEOm".ToCharArray());
			writer.Write(ProtocolVersion);

			BinaryFormatter formatter = new BinaryFormatter();
			formatter.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
			formatter.FilterLevel = TypeFilterLevel.Low;
			formatter.Serialize(writer.BaseStream, Blueprints);

			writer.Close();
		}

		public void DestroyObjects()
		{
			foreach (GameObject logicalObject in ActiveObjects)
			{
				logicalObject.Destroy();
			}
			ActiveObjects.Clear();
			ActivePhysicalObjects.Clear();
		}

		public void Destroy()
		{
			Resources.Objectmaps.Remove(ID);
		}

		public static implicit operator Objectmap(string str)
		{
			return Resources.Objectmaps[str];
		}
	}
}