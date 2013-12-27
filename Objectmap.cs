using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Stores a list of objects, that can be mass-created and removed.
    /// Convenient way of creating levels or maps in general.
    /// </summary>
    public class Objectmap
    {
        public string Id { get; private set; }

        private List<ObjectmapBlueprint> Blueprints;
        public List<LogicalObject> ActiveObjects = new List<LogicalObject>();

        internal Objectmap(string id, params ObjectmapBlueprint[] blueprints)
        {
            Id = id;
            Blueprints = new List<ObjectmapBlueprint>(blueprints);
        }

        public List<LogicalObject> CreateObjects()
        {
            List<LogicalObject> returnList = new List<LogicalObject>();
            foreach (ObjectmapBlueprint blueprint in Blueprints)
            {
                LogicalObject logicalObject = (LogicalObject)Activator.CreateInstance(blueprint.Type, blueprint.Arguments.ToArray());
                ActiveObjects.Add(logicalObject);
                returnList.Add(logicalObject);
            }
            return returnList;
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
