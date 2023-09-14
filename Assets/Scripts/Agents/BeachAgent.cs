using UnityEngine;
using System;
using System.Collections.Generic;

namespace Agents
{
    public class BeachAgent : BaseAgent
    {
        // The maximum height for a vertex to be considered an option for the beachs
        private float _maxHeight = 10f;
        // The minimum height for the beach
        private float _sealevel = 0.5f;
        // The flattening radius for the beach
        private int _flatRadius = 4;
        // The distance to walk per random walk step
        private int _inlandDistance = 5;
        // The maximum amount of random walks for the inland section
        private int _numberOfWalks = 100;
        // The maximum extra height for the beach
        private float _beachrange = 0.2f;
        // The total number of seprate beaches
        private int _numberOfBeaches;

        // A random number generator used here and there
        private readonly System.Random _rnd;

        // A list of all the vertices that are on the coast
        private List<Vector2Int> _verticesOnCoast;
        
        /// <summary>
        /// Constructor for the beach agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The depth of the terrain</param>
        /// <param name="tokens">The maximal number of coastal vertices in a single beach</param>
        /// <param name="numberOfBeaches">The total number of seperate beaches</param>
        public BeachAgent(Point[,] vertices, int width, int depth, int tokens, int numberOfBeaches, int inlandDitance, float seaLevel, float maxHeight) : base(vertices, width, depth, tokens)
        {
            _verticesOnCoast = GetCoastVertices();
            _rnd = new System.Random();
            
            _numberOfBeaches = numberOfBeaches;
            _maxHeight = maxHeight;
            _sealevel = seaLevel;
            _inlandDistance = inlandDitance;
        }

        /// <summary>
        /// Create a given amount of beaches
        /// </summary>
        /// <returns>The vertices of the terrain with beaches</returns>
        public override Point[,] DoAgentJob()
        {
            for (int i = 0; i < _numberOfBeaches; i++)
            {
                BeachGenerate();
            }
            return Vertices;
        }

        /// <summary>
        /// Create a single beach
        /// </summary>
        private void BeachGenerate()
        {
            // Return if there is no coastal vertex available
            if (_verticesOnCoast.Count == 0) return;
            // Otherwise pick a random coastal vertex to start
            Vector2Int beachPos = _verticesOnCoast[_rnd.Next(_verticesOnCoast.Count)];
            for (int i = 0; i < Tokens; i++)
            {
                // Try again if the coast is too high
                if (Vertices[beachPos.x, beachPos.y].Vertex.y > _maxHeight)
                {
                    // Remove this vertex from the coastal vertices
                    _verticesOnCoast.Remove(beachPos);
                    // Return if there is no more coastal vertex available
                    if (_verticesOnCoast.Count == 0) return;
                    beachPos = _verticesOnCoast[_rnd.Next(_verticesOnCoast.Count)];
                    continue;
                }
                // Flatten the area around the beach position
                Flatten(beachPos);
                // Find a random point inland
                Vector2Int inlandPos = FindInlandPoint(beachPos);
                // Random walk a given amount of times, flattening the area in the process
                for (int j = 0; j < _numberOfWalks; j++)
                {
                    // Try again if the inland position is too high
                    if (Vertices[inlandPos.x, inlandPos.y].Vertex.y > _maxHeight) break;
                    Flatten(inlandPos);
                    inlandPos = new Vector2Int(_rnd.Next(-1, 2) + inlandPos.x, inlandPos.y + _rnd.Next(-1, 2));
                    // Stop if the new position is no longer on land
                    if (!IsOnLand(inlandPos)) break;
                }
                // Continue with an adjecent point on the coast, that has not been processed earlier
                _verticesOnCoast.Remove(beachPos);
                bool newPos = false;
                for (int y = beachPos.y - 1; y <= beachPos.y + 1; y++)
                {
                    for (int x = beachPos.x - 1; x <= beachPos.x + 1; x++)
                    {
                        Vector2Int vert = new Vector2Int(x, y);
                        if (OnCoast(vert) && _verticesOnCoast.Contains(vert))
                        {
                            newPos = true;
                            beachPos = vert;
                        }
                    }
                }
                // If such a point doesnt exist, stop creating this beach
                if (!newPos) return;
                // Return if there is no more coastal vertex available
            }
        }

        /// <summary>
        /// Check if a given vertex is on the coast
        /// </summary>
        /// <param name="vertex">The vertex that may be on the coast</param>
        /// <returns>Whether or not the vertex lies on the coast</returns>
        private bool OnCoast(Vector2Int vertex)
        {
            // Check if the given vertex is on land
            if (Vertices[vertex.x, vertex.y].Vertex.y < _sealevel) return false;
            // Check if any of the surrounding vertices is in the sea
            for (int yy = vertex.y - 1; yy <= vertex.y + 1; yy++)
            {
                for (int xx = vertex.x - 1; xx <= vertex.x + 1; xx++)
                {
                    // Skip if the vertex is out of bounds
                    if (xx < 0 || xx >= Width || yy < 0 || yy >= Depth) continue;
                    // Return if at least one neighbouring vertex is below sea level.
                    if (Vertices[xx, yy].Vertex.y == 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Flatten the area around a given vertex
        /// </summary>
        /// <param name="vertex">The vertex which environment must be flattened</param>
        private void Flatten(Vector2Int vertex)
        {
            int rounded = (int)Math.Floor((double)_flatRadius / 2);
            // Iterate over all neighbouring vertices
            for (int xx = vertex.x - rounded; xx <= vertex.x + rounded; xx++)
            {
                for (int yy = vertex.y - rounded; yy <= vertex.y + rounded; yy++)
                {
                    // Skip vertices that are out of bounds
                    if (xx < 0 || xx >= Width || yy < 0 || yy >= Depth) continue;
                    // Skip vertices under the sea level as we are not interested in underwater landscapes
                    if (Vertices[xx, yy].Vertex.y < _sealevel || Vertices[xx, yy].Vertex.y > _maxHeight) continue;
                    // Set the height of the vertex to the sealevel plus a random height
                    Vertices[xx, yy] = new Point(new Vector3(Vertices[xx, yy].Vertex.x, _sealevel + (float)_rnd.NextDouble() * _beachrange, Vertices[xx, yy].Vertex.z));
                }
            }
        }
        /// <summary>
        /// Find an adjecent point that is inland
        /// </summary>
        /// <param name="vertex">A given coastal vertex</param>
        /// <returns>An adjecent inland vertex, or the same vertex if such vertex does not exist</returns>
        private Vector2Int FindInlandPoint(Vector2Int vertex)
        {
            // Find an adjecent point that is on land
            for (int i = 0; i < 10; i++)
            {
                Vector2Int inlandPos = new Vector2Int(_rnd.Next(-1,2) + vertex.x, vertex.y + _rnd.Next(-1, 2));
                // Skip vertices that are out of bounds
                if (inlandPos.x < 0 || inlandPos.x >= Width || inlandPos.y < 0 || inlandPos.y >= Depth) return vertex;
                // The point has to be different from the given point
                if (inlandPos.x == vertex.x && inlandPos.y == vertex.y) continue;
                // The point also must be on land
                if (Vertices[inlandPos.x, inlandPos.y].Vertex.y < _sealevel || Vertices[inlandPos.x, inlandPos.y].Vertex.y > 2) continue;
                // Find the vector from the original position to the new position
                Vector2Int vec = vertex - inlandPos;
                // Walk in the direction of the vector for a certain length
                inlandPos = vertex + _inlandDistance * vec;
                // Return if this position is inland
                if (IsOnLand(inlandPos)) return inlandPos;

            }
            // After too many failed attempts, return the coast pos
            return vertex;
        }

        /// <summary>
        /// Function that checks if a vertex is on land
        /// </summary>
        /// <param name="vertex">A given vertex</param>
        /// <returns>Whether or not that vertex is on land</returns>
        private bool IsOnLand(Vector2Int vertex)
        {
            return Vertices[vertex.x, vertex.y].Vertex.y >= _sealevel;
        }
    }
}
