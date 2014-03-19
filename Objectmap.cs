using System;
using System.Collections.Generic;
using System.IO;

namespace HatlessEngine
{
    /// <summary>
    /// Stores a list of objects, that can be mass-created and removed.
    /// Convenient way of creating levels or maps in general.
    /// </summary>
    public class Objectmap
    {
        public string Id { get; private set; }

		public static readonly ushort ProtocolVersion = 1;

        private List<ObjectBlueprint> Blueprints;
        public List<LogicalObject> ActiveObjects = new List<LogicalObject>();

        internal Objectmap(string id, params ObjectBlueprint[] blueprints)
        {
            Id = id;
            Blueprints = new List<ObjectBlueprint>(blueprints);
        }

		[Obsolete("Not yet implemented.")]
		internal Objectmap(string id, string filename)
		{
			Id = id;
			Blueprints = new List<ObjectBlueprint>();

			BinaryReader reader = new BinaryReader(Resources.GetStream(filename));

			if (reader.ReadChars(4) != "HEOm".ToCharArray())
				throw new ProtocolMismatchException("The file's magic number is not 'HEOm' (HatlessEngine Objectmap)");
				
			if (reader.ReadUInt16() != ProtocolVersion)
				throw new ProtocolMismatchException("The file's protocol version is not equal to the required one (" + ProtocolVersion.ToString() + ")");

			//Type.
		}

        public List<LogicalObject> CreateObjects()
        {
            List<LogicalObject> returnList = new List<LogicalObject>();
            foreach (ObjectBlueprint blueprint in Blueprints)
            {
                LogicalObject logicalObject = (LogicalObject)Activator.CreateInstance(blueprint.Type, blueprint.Arguments.ToArray());
                ActiveObjects.Add(logicalObject);
                returnList.Add(logicalObject);
            }
            return returnList;
        }

		[Obsolete("Not yet implemented.")]
		public void WriteToFile(string filename)
		{
			BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
			writer.Write("HEOm".ToCharArray());
			writer.Write(ProtocolVersion);

			//amount of blueprints

			//objecttype
			//amount of arguments

			//argumenttype 
			//amount of arguments (if zero, read in value / else, next type...)
			//argumenttype
			//argumentvalue

		}

        public void DestroyObjects()
        {
            foreach (LogicalObject logicalObject in ActiveObjects)
            {
                logicalObject.Destroy();
            }
            ActiveObjects.Clear();
        }
    }
}
