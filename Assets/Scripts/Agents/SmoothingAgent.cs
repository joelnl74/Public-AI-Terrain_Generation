using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication.ExtendedProtection;
using UnityEngine;
using Utils;

namespace Agents
{
    public class SmoothingAgent : BaseAgent
    {
        /// <summary>
        /// Constructor for the smoothing agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The height of the terrain</param>
        /// <param name="tokens">The number of iterations for the smooting agent</param>
        public SmoothingAgent(Point[,] vertices, int width, int depth, int tokens) : base(vertices, width, depth, tokens)
        {
        }

        /// <summary>
        /// Smooches the terrain
        /// </summary>
        /// <returns>Vertices of the smoothed terrain</returns>
        public override Point[,] DoAgentJob()
        {         
            for (var i = 0; i < Tokens; i++)
            {
                foreach (var pos in Vertices)
                {
                    Vector3 vertex = pos.Vertex;
                    // Skip vertices that are underwater
                    if (vertex.y < 1) continue;
                    // Find the 4 neighbouring positions of the current vertex
                    var neighbours = GetNeighbours((int)vertex.x, (int)vertex.z, Width, Depth);
                
                    SmoothTerrain(neighbours, new Vector2Int((int)vertex.x, (int)vertex.z));
                }
            }
            
            return Vertices;
        }

        /// <summary>
        /// Smooches a single vertex based on its neighbours
        /// </summary>
        /// <param name="neighbours">The list of neighbours of the vertex</param>
        /// <param name="position">The vertex that needs smoothing</param>
        private void SmoothTerrain(Dictionary<Direction, Point> neighbours, Vector2Int position)
        {
            // Get the average height of all neighbours, and set the height of the given vertex to that
            var averageHeight = neighbours.Values.Average(x => x.Vertex.y);
            Vertices[position.x, position.y].Vertex.y = averageHeight;
        }
    }
}
