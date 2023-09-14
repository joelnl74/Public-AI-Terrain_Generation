using UnityEngine;
using System.Collections.Generic;

namespace Agents
{
    public class RiverAgent : BaseAgent
    {
        private int _amountOfRivers;
        private int _attempts;

        private readonly List<Vector2Int> _coastVertices;
        
        
        public RiverAgent(Point[,] vertices, int width, int depth, int tokens) : base(vertices, width, depth, tokens)
        {
            _coastVertices = TerrainManager.Instance.VerticesOnCoast;
        }
        
        public override Point[,] DoAgentJob()
        {
            for (var i = 0; i < _attempts; i++)
            {
                // Get random coast line point.
                var coastPoint = _coastVertices[Random.Range(0, TerrainManager.Instance.VerticesOnCoast.Count)];
                var coastVertex = Vertices[coastPoint.x, coastPoint.y];
            }


            return Vertices;
        }
    }
}