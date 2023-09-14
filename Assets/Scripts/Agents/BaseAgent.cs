using UnityEngine;
using System.Collections.Generic;
using Utils;

namespace Agents
{
    public abstract class BaseAgent
    {
        protected readonly int Width;
        protected readonly int Depth;
        
        protected int Tokens;
        
        protected readonly Point[,] Vertices;

        protected BaseAgent(Point[,] vertices, int width, int depth, int tokens)
        {
            Width = width;
            Depth = depth;
            Tokens = tokens;

            Vertices = vertices;
        }
        
        public abstract Point[,] DoAgentJob();

        public List<Vector2Int> GetMountainVertices()
        {
            return TerrainManager.Instance.MountainVertices;
        }
        
        public List<Vector2Int> GetCoastVertices()
        {
            return TerrainManager.Instance.VerticesOnCoast;
        }

        protected Dictionary<Direction, Point> GetNeighbours(int x, int z, int width, int depth)
        {
            var neighbours = new Dictionary<Direction, Point>();

            if (z + 1 < depth)
            {
                var north = Vertices[x, z + 1];
                neighbours.Add(Direction.North, north);
            }

            if (z - 1 > 0)
            {
                var south = Vertices[x, z - 1];
                neighbours.Add(Direction.South, south);
            }

            if (x + 1 < width)
            {
                var east = Vertices[x + 1, z];
                neighbours.Add(Direction.East, east);
            }

            if (x - 1 > 0)
            {
                var west = Vertices[x - 1, z];
                neighbours.Add(Direction.West, west);
            }

            return neighbours;
        }
    }
}
