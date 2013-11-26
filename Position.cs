using System;

namespace HatlessEngine
{
    /// <summary>
    /// A simple struct describing the x and y of a position.
    /// </summary>
    public struct Position
    {
        public float X;
        public float Y;

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float DistanceToPosition(Position pos)
        {
            return (float)Math.Sqrt(Math.Pow(pos.X - X, 2) + Math.Pow(pos.Y - Y, 2));  
        }
        public Angle AngleToPosition(Position pos)
        {
            return new Angle((float)Math.Atan2(pos.X - X, Y - pos.Y), true);
        }

        public static Position operator +(Position position1, Position position2)
        {
            return new Position(position1.X + position2.X, position1.Y + position2.Y);
        }
        public static Position operator -(Position position1, Position position2)
        {
            return new Position(position1.X - position2.X, position1.Y - position2.Y);
        }
        public static Position operator *(Position position1, Position position2)
        {
            return new Position(position1.X * position2.X, position1.Y * position2.Y);
        }
        public static Position operator /(Position position1, Position position2)
        {
            return new Position(position1.X / position2.X, position1.Y / position2.Y);
        }

        public static Position operator +(Position position, Speed speed)
        {
            return new Position(position.X + speed.X, position.Y + speed.Y);
        }
        public static Position operator -(Position position, Speed speed)
        {
            return new Position(position.X - speed.X, position.Y - speed.Y);
        }
        public static Position operator *(Position position, Speed speed)
        {
            return new Position(position.X * speed.X, position.Y * speed.Y);
        }
        public static Position operator /(Position position, Speed speed)
        {
            return new Position(position.X / speed.X, position.Y / speed.Y);
        }

        public static Position operator +(Position position, float value)
        {
            return new Position(position.X + value, position.Y + value);
        }
        public static Position operator -(Position position, float value)
        {
            return new Position(position.X - value, position.Y - value);
        }
        public static Position operator *(Position position, float value)
        {
            return new Position(position.X * value, position.Y * value);
        }
        public static Position operator /(Position position, float value)
        {
            return new Position(position.X / value, position.Y / value);
        }

        public static implicit operator SFML.Window.Vector2f(Position pos)
        {
            return new SFML.Window.Vector2f(pos.X, pos.Y);
        }
    }
}
