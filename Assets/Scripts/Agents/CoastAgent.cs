using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Agents
{

    public class CoastAgent : BaseAgent
    {
        // The margin to the edge of the map where land cannot be placed
        private readonly int _borderSize;

        // The radius of each agent, where he can place terrain
        private readonly int _radius = 30;
                
        // Random number generator used on various places
        private readonly System.Random _rnd;
        // The maximum amount of tokens per agent
        private readonly int _limit;
        // The initial height of all land vertices
        private readonly int _startHeight = 3;
        // A list containing the coördinates of all vertices that are located on the coast       
        private List<Vector2Int> _verticesOnCoast = new List<Vector2Int>();
        
        /// <summary>
        /// Constructor of the agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width (in vertices) of the terrain</param>
        /// <param name="depth">The depth (in vertices) of the terrain</param>
        /// <param name="tokens">The maximum amount of vertices that the land will consist of</param>
        /// <param name="borderSize">The margin near the edge of the map</param>
        public CoastAgent(Point[,] vertices, int width, int depth, int tokens, int borderSize) : base(vertices, width, depth, tokens)
        {
            _borderSize = borderSize;
            _rnd = new System.Random();
            _limit = tokens / 4096;
        }

        /// <summary>
        /// Execute the coastline agent
        /// </summary>
        /// <returns>The updated vertices of the terrain</returns>
        public override Point[,] DoAgentJob()
        {
            // Create a single vertex of land to initialize the algorithm, in the center of the terrein
            Vertices[Width / 2, Depth / 2] = new Point(new Vector3(Width / 2, _startHeight, Depth / 2));
            
            // Add this vertex to the vertices on the coast
            _verticesOnCoast.Add(new Vector2Int(Width / 2, Depth / 2));
            // Call CoastlineGenerate, which is a recursive function that expands the coastline
            CoastlineGenerate(new Vector2Int(Width / 2, Depth / 2), Tokens);            
            return Vertices;
        }

        /// <summary>
        /// Return the list of vertices on the coastline, which is used by other agents
        /// </summary>
        /// <returns>The list of vertices on the coastline</returns>
        public List<Vector2Int> RetrieveCoastVertices()
        {
            return _verticesOnCoast;
        }

        /// <summary>
        /// Recursively expand the coastline, starting at the center
        /// </summary>
        /// <param name="agent">The location of the agent</param>
        /// <param name="tokens">The amount of tokens in the possession of the agent</param>
        private void CoastlineGenerate(Vector2Int agent, int tokens)
        {
            // List all vertices on the coast that are in the radius of this agent
            var inRadius = _verticesOnCoast.Where(pos => pos.x < agent.x + _radius && pos.x > agent.x - _radius && pos.y < agent.y + _radius && pos.y > agent.y - _radius).ToList();
            // If the agent has more tokens than its limit, create two sub-agents who recursively call this function, and give each half of this agents tokens
            if (tokens >= _limit)
            {
                for (var i = 0; i < 2; i++)
                {
                    // If there is no coastal vertex in the radius of the agent, pick any coastal vertex
                    CoastlineGenerate(inRadius.Count == 0 ? _verticesOnCoast[_rnd.Next(_verticesOnCoast.Count)] : inRadius[_rnd.Next(inRadius.Count)], tokens / 2);
                }
            }
            // Otherwise the coastline can be generated
            else
            {
                // Check if the agent is still located on the coast
                // If not, try walking to the coastline by moving in a random direction
                if (!OnCoast(agent.x, agent.y))
                {
                    agent = TryWalk(agent);
                    // Give up after too many failed walks
                    if (agent.x == -1 && agent.y == -1) return;
                }
                // Create a repellor and an attractor point close to the agent
                var repellor = new Vector2(_rnd.Next(-10, 11), _rnd.Next(-10, 11)) + agent;
                var attractor = new Vector2(_rnd.Next(-10, 11), _rnd.Next(-10, 11)) + agent;

                //Change attractor so that it will be in the opposite quadrant of the repellor
                if (Mathf.Sign(attractor.x) == Mathf.Sign(repellor.x)) attractor.x *= -1;
                if (Mathf.Sign(attractor.y) == Mathf.Sign(repellor.y)) attractor.y *= -1;

                // Spend all of this agents tokens extending the coastline
                for(int t = 0; t < tokens; t++)
                {
                getVert:
                    // If the agent no longer has any coastal vertices in its radius, stop iterating
                    if (inRadius.Count == 0)
                    {
                        break;
                    }
                    var vertex = inRadius[_rnd.Next(inRadius.Count)];
                    // If the vertex no longer lies on the coast, try again and remove it from the inRadius list
                    if (!OnCoast(vertex.x, vertex.y))
                    {
                        inRadius.Remove(vertex);
                        _verticesOnCoast.Remove(vertex);
                        goto getVert;
                    }
                    // Find the adjecent coastal tile with the highest score, and elevate it above sealevel
                    var highestScore = int.MinValue;
                    var highestVertex = new Vector2Int(0, 0);

                    for (var y = vertex.y - 1; y <= vertex.y + 1; y++)
                    {
                        for (var x = vertex.x - 1; x <= vertex.x + 1; x++)
                        {
                            // Skip if the vertex is out of bounds
                            if (x < 0 + _borderSize || x >= Width - _borderSize || y < 0 + _borderSize || y >= Depth - _borderSize) continue;
                            // Skip if the vertex already lies above sealevel
                            if (Vertices[x, y].Vertex.y > 0) continue;
                            // Calculate the score of the vertex
                            var score = (int)(Mathf.Pow(repellor.x - x, 2) + Mathf.Pow(repellor.y - y, 2)                       //Distance to repellor
                                              - Mathf.Pow(repellor.x - x, 2) + Mathf.Pow(repellor.y - y, 2) +                   //Distance to attractor
                                              3 * Mathf.Pow(Mathf.Min(Mathf.Min(x, Width - x), Mathf.Min(y, Depth - y)), 2));   //Shortest distance to the edge
                            // Replace the previous vertex if the score of this vertex is better
                            if (score > highestScore)
                            {
                                highestScore = score;
                                highestVertex = new Vector2Int(x, y);
                            }
                        }
                    }
                    // Elevate the vertex with the highest score above sealevel, and add it to the list of vertices that are on the coast
                    _verticesOnCoast.Add(highestVertex);
                    inRadius.Add(highestVertex);
                    Vertices[highestVertex.x, highestVertex.y] = new Point(new Vector3(highestVertex.x, _startHeight, highestVertex.y));
                }
            }
        }

        /// <summary>
        /// Check if a given vertex is on the coast
        /// </summary>
        /// <param name="x">The x coordinate of the vertex</param>
        /// <param name="y">The y coordinate of the vertex</param>
        /// <returns>whether or not the given vertex is on land, inside the terrain margin, and adjecent to sea</returns>
        private bool OnCoast(int x, int y)
        {
            // Check if the given vertex is on land
            if (Vertices[x, y].Vertex.y < 1) return false;
            // Check if any of the surrounding vertices is in the sea
            for (var yy = y - 1; yy <= y + 1; yy++)
            {
                for (var xx = x - 1; xx <= x + 1; xx++)
                {
                    // Skip if the vertex is out of bounds
                    if (xx < 0 + _borderSize || xx >= Width - _borderSize || yy < 0 + _borderSize || yy >= Depth - _borderSize) continue;
                    // Return if at least one neighbouring vertex is below sea level.
                    if (Vertices[xx, yy].Vertex.y == 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Try walking towards the coast from a given vertex position
        /// </summary>
        /// <param name="agent">The initial position of the vertex</param>
        /// <returns>A coastal vertex that the agent walked to, or a negative vertex if the walked failed</returns>
        private Vector2Int TryWalk(Vector2Int agent)
        {
            // Pick a random direction to walk
            var direction = GetUnitVector();
            var newAgent = new Vector2Int(-1, -1);  
            for (var i = 0; i < 5; i++)
            {
                newAgent = Walk(agent, direction);
                // If a random walk does not lead to coast, try again with a new direction
                if (newAgent.x != -1 && newAgent.y != -1)
                {
                    return newAgent;
                }
                direction = GetUnitVector();
            }
            // After too many failed attempts, give up
            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// Walk from a position to a given direction
        /// </summary>
        /// <param name="position">The initial position</param>
        /// <param name="direction">The direction</param>
        /// <returns>A coastal vertex that the agent walked to, or a negative vertex if the walked failed</returns>
        private Vector2Int Walk(Vector2Int position, Vector2 direction)
        {
            Vector2 nonRoundedPosition = position;          
            for (var i = 0; i < Mathf.Max(Width, Depth); i++)
            {
                // Ignote vertices that are out of bounds
                if (position.x < 0 || position.x >= Width || position.y < 0 || position.y >= Depth)
                {
                    break;
                }
                if (OnCoast(position.x, position.y)) return position;
                nonRoundedPosition += direction;
                position = new Vector2Int(Mathf.RoundToInt(nonRoundedPosition.x), Mathf.RoundToInt(nonRoundedPosition.y));
            }
            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// Helper function that returns a random unit vector
        /// </summary>
        /// <returns>A random unit vector</returns>
        private Vector2 GetUnitVector()
        {
            var unitVector = new Vector2(0, 0);
            
            while (unitVector.x == 0 && unitVector.y == 0)
            {
                unitVector = new Vector2((float)(_rnd.NextDouble() * 2 - 1), (float)(_rnd.NextDouble() * 2 - 1));
            }
            return unitVector.normalized;
        }
    }
}