using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Agents
{
    public class LavaAgent : BaseAgent
    {
        // A list of the locations of all volcanoes
        private List<Vector2Int> _volcanoes;
        // A list with the caldera widths of all volcanoes
        private List<float> _calderaWidths;
        // The level of noise for the lava river
        private float _noiseRatio = 1f;

        // A hashset containing all vertices that have already been noised by this algorithm
        private HashSet<Vector2Int> _alreadyNoised;
        // A list of vertices with the path of the lava river
        private List<Vector2Int> _path;

        // A random number generator used here and there
        private System.Random _rnd;

        /// <summary>
        /// The constructor for the Lava Agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The depth of the terrain</param>
        /// <param name="tokens">The number of lava rivers</param>
        /// <param name="volcanoes">A list of all volcanoes</param>
        /// <param name="calderaWidths">A list of all caldera widths</param>
        public LavaAgent(Point[,] vertices, int width, int depth, int tokens, List<Vector2Int> volcanoes, List<float> calderaWidths) : base(vertices, width, depth, tokens)
        {
            _rnd = new System.Random();
            _volcanoes = volcanoes;
            _calderaWidths = calderaWidths;
            _alreadyNoised = new HashSet<Vector2Int>();
            _path = new List<Vector2Int>();
        }

        /// <summary>
        /// Generate a number of lava rivers
        /// </summary>
        /// <returns>The vertices of the terrain with lava rivers</returns>
        public override Point[,] DoAgentJob()
        {
            // Generate a number of lava rivers equal to the amount of tokens
            for (int t = 0; t < Tokens; t++)
            {
                // Break if there are no more volcanoes left
                if (_volcanoes.Count == 0) break;
                // Otherwise pick a random volcano
                int index = _rnd.Next(_volcanoes.Count);
                Vector2Int volcano = _volcanoes[index];
                float calderaWidth = _calderaWidths[index];
                // Remove the volcano from the list so that it can not be picked again
                _volcanoes.RemoveAt(index);
                _calderaWidths.RemoveAt(index);
                // Find the caldera point that is the lowest
                float lowestCaldera = float.MaxValue;
                Vector2 floatPoint = Vector2.zero;
                Vector2Int point = Vector2Int.zero;
                Vector2 direction = Vector2.zero;
                foreach (Point p in Vertices)
                {
                    Vector3 vertex = p.Vertex;
                    Vector2Int vertexPos = new Vector2Int((int)vertex.x, (int)vertex.z);
                    if (vertex.y < 1) continue;
                    if (Vector2Int.Distance(volcano, vertexPos) == (int)calderaWidth + 5)
                    {
                        if (vertex.y < lowestCaldera)
                        {
                            lowestCaldera = vertex.y;
                            point = vertexPos;
                        }
                    }
                }
                floatPoint = point;
                direction = volcano - point;
                direction.Normalize();
                // Do one step away from the caldera
                floatPoint = floatPoint + direction;
                point = new Vector2Int((int)floatPoint.x, (int)floatPoint.y);
                _path.Add(point);
                // Carve a path that goes down
                // For loop is set to 1000, but will most likely break before that
                for (int i = 0; i < 1000; i++)
                {
                    Vector2Int newPoint = LowestSurroundingPoint(point);
                    // Stop if there are no lower neightbours
                    _path.Add(newPoint);
                    // Allow going up with a small angle if the lava hasn't hit the sea yet
                    if (Vertices[newPoint.x, newPoint.y].Vertex.y < Vertices[point.x, point.y].Vertex.y || (Vertices[newPoint.x, newPoint.y].Vertex.y - 2f < Vertices[point.x, point.y].Vertex.y && Vertices[newPoint.x, newPoint.y].Vertex.y > 0.0f)) point = newPoint;
                    else break;
                }
                for (int i = 0; i < _path.Count; i++)
                {
                    NoisePoints(_path[i]);
                }
            }
            return Vertices;
        }

        /// <summary>
        /// Adds noise to all points around the center
        /// </summary>
        /// <param name="center">A vertex in the path of the lava river</param>
        private void NoisePoints(Vector2Int center)
        {
            for(int x = center.x - 3; x <= center.x + 3; x++)
            {
                for (int y = center.y - 3; y <= center.y + 3; y++)
                {
                    if (_alreadyNoised.Contains(new Vector2Int(x, y))) continue;
                    Vertices[x, y].Vertex.y = Vertices[x, y].Vertex.y + (float)_rnd.NextDouble() * _noiseRatio;
                    
                    Vertices[x, y].Color = new Color(255, 153, 0);

                    _alreadyNoised.Add(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Find the point on a given center that has the lowest height
        /// </summary>
        /// <param name="center">A vertex in the path of the lava river</param>
        /// <returns>The adjecent point that is lowest</returns>
        private Vector2Int LowestSurroundingPoint(Vector2Int center)
        {
            float lowestHeight = float.MaxValue;
            Vector2Int lowestPoint = Vector2Int.zero;
            for (int x = center.x - 1; x <= center.x + 1; x++)
            {
                for (int y = center.y - 1; y <= center.y + 1; y++)
                {
                    // Ignore the center itself
                    if (x == center.x && y == center.y) continue;
                    if (Vertices[x, y].Vertex.y < lowestHeight)
                    {
                        lowestHeight = Vertices[x, y].Vertex.y;
                        lowestPoint = new Vector2Int(x, y);
                    }

                }
            }
            return lowestPoint;
        }
    }
}
