using System;

namespace HatlessEngine
{
    /// <summary>
    /// Base for draw transformation structs.
    /// </summary>
    public class DrawTransformation
    {
        public enum TransformationType { ScaleAll = 0, ScaleAxes = 1, Rotate = 2 }
        public TransformationType Type;
        public object Argument1;
        public object Argument2;

        private DrawTransformation(TransformationType type, object argument1, object argument2 = null)
        {
            Type = type;
            Argument1 = argument1;
            Argument2 = argument2;
        }

        public static DrawTransformation Scale(float scale)
        {
            return new DrawTransformation(TransformationType.ScaleAll, scale);
        }
        public static DrawTransformation Scale(float xScale, float yScale)
        {
            return new DrawTransformation(TransformationType.ScaleAxes, xScale, yScale);
        }

        public static DrawTransformation Rotate(Position axis, float angle)
        {
            return new DrawTransformation(TransformationType.Rotate, axis, angle);
        }
    }
}
