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
    public class Objectmap
    {
        public string Id { get; private set; }

		/// <summary>
		/// protocol version changes on changes to ObjectBlueprint.
		/// </summary>
		public static readonly ushort ProtocolVersion = 1;

		public List<ObjectBlueprint> Blueprints;
        public List<LogicalObject> ActiveObjects = new List<LogicalObject>();
        internal List<PhysicalObject> ActivePhysicalObjects = new List<PhysicalObject>();

        internal Objectmap(string id, params ObjectBlueprint[] blueprints)
        {
            Id = id;
            Blueprints = new List<ObjectBlueprint>(blueprints);
        }
			
		internal Objectmap(string id, string filename)
		{
			Id = id;

			BinaryReader reader = new BinaryReader(Resources.GetStream(filename));

			if (new String(reader.ReadChars(4)) != "HEOm")
				throw new ProtocolMismatchException("The file's magic number is not 'HEOm' (HatlessEngine Objectmap)");

			if (reader.ReadUInt16() != ProtocolVersion)
				throw new ProtocolMismatchException("The file's protocol version is not equal to the required one (" + ProtocolVersion.ToString() + ")");
				
			BinaryFormatter formatter = new BinaryFormatter();
			Blueprints = (List<ObjectBlueprint>)formatter.Deserialize(reader.BaseStream);

			reader.Close();
		}

        public List<LogicalObject> CreateObjects()
        {
            List<LogicalObject> returnList = new List<LogicalObject>();
            foreach (ObjectBlueprint blueprint in Blueprints)
            {
                LogicalObject logicalObject = (LogicalObject)Activator.CreateInstance(blueprint.Type, blueprint.Arguments);
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
            foreach (LogicalObject logicalObject in ActiveObjects)
            {
                logicalObject.Destroy();
            }
            ActiveObjects.Clear();
            ActivePhysicalObjects.Clear();
        }
    }
}
