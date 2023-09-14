using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public enum Direction
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3,
    }

    public static class DirectionUtils
    {
        public static Direction GetRandomDirection()
        {
            var index = Random.Range(0, Enum.GetValues(typeof(Direction)).Length);

            return (Direction) index;
        }

        public static Vector2Int GetRandomDirectionCoordinates(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Vector2Int(0, 1);
                case Direction.South:
                    return new Vector2Int(0, -1);
                case Direction.West:
                    return new Vector2Int(-1, 0);
                case Direction.East:
                    return new Vector2Int(1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static bool CanProceed(Point currentPoint, int width, int height)
        {
            return !(currentPoint.Vertex.x + 1 > width) && !(currentPoint.Vertex.z + 1 > height) && !(currentPoint.Vertex.x - 1 < 0) && !(currentPoint.Vertex.z - 1 < 0);
        }
    }
}
