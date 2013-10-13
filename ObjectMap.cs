using System;
using System.Collections.Generic;

namespace HatlessEngine
{
    /// <summary>
    /// Stores a list of objects, that can be mass-created and removed.
    /// Convenient way of creating levels or maps in general.
    /// </summary>
    public class ObjectMap
    {
        public string Id { get; private set; }

        private List<ObjectBlueprint> Blueprints;
        public List<LogicalObject> ActiveObjects = new List<LogicalObject>();

        public ObjectMap(string id, params ObjectBlueprint[] objects)
        {
            Id = id;
            Blueprints = new List<ObjectBlueprint>(objects);
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
