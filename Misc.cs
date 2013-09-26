using System;

namespace HatlessEngine
{
    public static class Misc
    {
        /// <summary>
        /// Returns whether a random chance is met.
        /// Explained: chance in values (1 in 100) chance of returning true
        /// </summary>
        /// <returns>Whether chance is met</returns>
        public static bool Chance(int values, int chance = 1)
        {
            int result = new Random().Next(values);
            if (result < chance)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks whether a type inherits from another type.
        /// </summary>
        /// <param name="type">Type (child).</param>
        /// <param name="inheritedType">Type to check for (parent).</param>
        /// <returns>Whether given type inherits from checkType.</returns>
        public static bool CheckInheritance(Type type, Type inheritedType)
        {
            if (type == inheritedType)
                return true;            

            Type currentBase = type.BaseType;
            while (currentBase != null)
            {
                if (currentBase == inheritedType)
                    return true;

                currentBase = currentBase.BaseType;
            }

            return false;
        }
    }
}
