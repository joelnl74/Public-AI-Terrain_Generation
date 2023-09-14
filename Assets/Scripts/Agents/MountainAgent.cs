using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Agents
{
    public class MountainAgent : BaseAgent
    {
        // The lower and upper bound for the number of mountains ranges
        private readonly int _minAmountOfMountains;
        private readonly int _maxAmountOfMountains;

        // The lower and upper bound for the length of each mountain range
        private readonly int _minLength;
        private readonly int _maxLength;

        // The max height for each mountain
        private readonly int _maxHeight;
        // The maximum amount of deviation for the height (divided by 2)
        private const int _heightVariation = 10;

        // The width of each mountain, this also affects the slope
        private readonly int _mountainWidth;

        // A small noising effect for the slopes of the mountains
        private const float _noiseRatio = 0.1f;

        // A random number generator that will be used here and there
        private System.Random _rnd;

        // A list of vertices containing all points above a certain altitute
        private List<Vector2Int> _mountainVertices = new List<Vector2Int>();
        // A list containing the coordinates of all mountaintops
        private List<Vector2Int> _mountainTops = new List<Vector2Int>();
        // A list containing all points that lie on the coast
        private List<Vector2Int> _verticesOnCoast;

        /// <summary>
        /// Constructor of the mountain agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The depth of the terrain</param>
        /// <param name="tokens">Does nothing, will be initialized later</param>
        /// <param name="mtnHeight">The maximum height of each mountain</param>
        /// <param name="tokens">The width of each mountain</param>
        /// <param name="minLength">The minimum length of a mountain range</param>
        /// <param name="maxLength">The maximum length of a mountain range</param>
        /// <param name="minAmountOfMountains">The minimum amount of mountain ranges</param>
        /// <param name="maxAmountOfMountains">The maximum amount of mountain ranges</param>
        public MountainAgent(Point[,] vertices, int width, int depth, int tokens, int mtnHeight, int mtnWidth, int minLength, int maxLength, int minAmountOfMountains, int maxAmountOfMountains) : base(vertices, width, depth, tokens)
        {
            _maxHeight = mtnHeight;
            _mountainWidth = mtnWidth;
            _maxLength = maxLength;
            _minLength = minLength;
            _minAmountOfMountains = minAmountOfMountains;
            _maxAmountOfMountains = maxAmountOfMountains;
            _verticesOnCoast = GetCoastVertices();        
            _rnd = new System.Random();
        }

        /// <summary>
        /// Generate a number of mountain ranges
        /// </summary>
        /// <returns>Vertices of the terrain with mountain ranges</returns>
        public override Point[,] DoAgentJob()
        {
            // Decide on how many mountain ranges should be generated
            var amountOfMountains = Random.Range(_minAmountOfMountains, _maxAmountOfMountains);

            for (var i = 0; i < amountOfMountains; i++)
            {
                // Set the amount of tokens to the (maximum) length of the mountain range
                Tokens = Random.Range(_minLength, _maxLength);
                GenerateMountain();
            }
            // Fix the slopes for all vertices close to a mountain
            ElevateVertices();
            return Vertices;
        }

        /// <summary>
        /// Generate a single mountain range
        /// </summary>
        private void GenerateMountain()
        {
            // Find a point that is inland
            Point point = new Point(Vector3.zero);
            while (point.Vertex.y < 1) point = Vertices[_rnd.Next(Width),_rnd.Next(Depth)];
            Vector2 position = new Vector2((int)point.Vertex.x, (int)point.Vertex.z);
            Vector2Int intPosition = new Vector2Int((int)position.x,(int)position.y);
            // Make sure that the mountainrange is moving away from the coast
            Vector2Int closestCoast = GetClosestCoast(intPosition);
            Vector2 initialDirection = (Vector2)(intPosition - closestCoast);
            initialDirection.Normalize();
            // Introduce more randomness to the initial direction so it will go at most perpendicular to the closest coast vertex
            Vector2 newDirection = GetDirectionInAngle(initialDirection, 180);

            for (var i = 0; i < Tokens; i++)
            {

                // Try walking in the direction of the mountain range
                position += newDirection;
                intPosition = new Vector2Int((int)position.x, (int)position.y);
                var vertex = Vertices[intPosition.x, intPosition.y].Vertex;
                // Return if a vertex is below sealevel
                if (vertex.y <= 1)
                {
                    return;
                }
                // Only save the height of 1 in 20 vertices to improve performance
                if (i % 20 == 0)
                {
                    // Increase the height of the vertex to a number between the max height, and the max height minus the bound
                    float height = _maxHeight - (float)_rnd.NextDouble() * _heightVariation;
                    Vertices[intPosition.x, intPosition.y].Vertex = new Vector3(vertex.x, height, vertex.z);
                    // If the mountaintop is too close to the coast, abort and try again
                    // Unless it is the first round, in which case the algorithm can try again
                    if (DistanceToCoast(intPosition) < _mountainWidth / 2)
                    {
                        if (i == 0) GenerateMountain();
                        return;
                    }
                    _mountainTops.Add(intPosition);
                }
                // Change direction every 50 tokens
                if (i % 50 == 0) newDirection = GetDirectionInAngle(newDirection, 90);
            }
        }
        
        /// <summary>
        /// Elevate the height of all vertices near at least 1 mountain
        /// </summary>
        private void ElevateVertices()
        {
            foreach (Point p in Vertices)
            {
                Vector3 vertex = p.Vertex;
                Vector2Int position = new Vector2Int((int)vertex.x, (int)vertex.z);
                // Skip if the vertex is underwater
                if (vertex.y < 1) continue;
                // Find and save all mountaintops that are in rance
                List<Vector2Int> closestMountains = new List<Vector2Int>();
                List<float> distances = new List<float>();
                List<float> normalizedDistances = new List<float>();
                foreach (Vector2Int mountainTop in _mountainTops)
                {
                    // Calculate the distance between the mountaintop and the vertex
                    float distance = Vector2Int.Distance(position, mountainTop);
                    // IF the distance is shorter than the width add it to the list
                    if (distance < _mountainWidth)
                    {
                        closestMountains.Add(mountainTop);
                        distances.Add(distance);
                    }
                }
                if (closestMountains.Count == 0) continue;
                // Normalize the list of distances after inverting them, so that mountains that are close by have more influence
                float sumList = 0;
                for (int i = 0; i < distances.Count; i++)
                {
                    float reverseDistance = _mountainWidth - distances[i];
                    sumList += reverseDistance;
                    normalizedDistances.Add(_mountainWidth - distances[i]);
                }
                for (int i = 0; i < distances.Count; i++)
                {
                    normalizedDistances[i] *= 1 / sumList;
                }
                // Calculate the (weighted) height of all mountaintop vertices nearby
                float totalHeight = 0;
                for (int i = 0; i < closestMountains.Count; i++)
                {
                    float mountainHeight = Vertices[closestMountains[i].x, closestMountains[i].y].Vertex.y;
                    totalHeight += (mountainHeight - distances[i] * (mountainHeight / _mountainWidth)) * normalizedDistances[i];
                }
                // Set the new height of a vertex to the old height, plus the height relative to the mountaintops, plus a noise value
                Vertices[position.x, position.y] = new Point(new Vector3(position.x, Vertices[position.x, position.y].Vertex.y + totalHeight + (((float)_rnd.NextDouble() * _noiseRatio) - (_noiseRatio / 2)) , position.y));
                // Save vertices that are high enough in the _mountainVertices list
                if (totalHeight > 2)
                {
                    _mountainVertices.Add(new Vector2Int(position.x, position.y));
                }
            }
        }
        
        /// <summary>
        /// Helper function that returns a direction vector in an angle of the given direction
        /// </summary>
        /// <param name="initialDirection">The given direction vector</param>
        /// <param name="coneSize">The maximum angle between the input and return vector</param>
        /// <returns>A vector with a new direction</returns>
        private Vector2 GetDirectionInAngle(Vector2 initialDirection, float coneSize)
        {
            // Get the angle between the initial vector and an offset vector
            float angle = Vector2.Angle(Vector2.right, initialDirection);
            float newAngle = (float)_rnd.NextDouble() * coneSize - (coneSize / 2) + angle;
            // Return the vector with the new direction
            return new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
        }

        /// <summary>
        /// Helper function that returns the distance from a given vertex to the coast
        /// </summary>
        /// <param name="position">A vertex on the terrain</param>
        /// <returns>The distance of the given vertex to the coast</returns>
        private float DistanceToCoast(Vector2Int position)
        {
            // Find the distance to the coast
            float shortestDistance = float.MaxValue;
            foreach (Vector2Int coastal in _verticesOnCoast)
            {
                float distance = Vector2Int.Distance(position, coastal);
                if (distance < shortestDistance) shortestDistance = distance;
            }
            return shortestDistance;
        }

        /// <summary>
        /// Helper function that returns the coastal vertex that is closest to the given vertex
        /// </summary>
        /// <param name="position">A vertex on the terrain</param>
        /// <returns>The coastal vertex that is closes to the given vertex</returns>
        private Vector2Int GetClosestCoast(Vector2Int position)
        {
            float shortestDistance = float.MaxValue;
            Vector2Int closestVector = Vector2Int.zero;
            foreach (Vector2Int coastal in _verticesOnCoast)
            {
                float distance = Vector2Int.Distance(position, coastal);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestVector = coastal;
                }
            }
            return closestVector;
        }

        /// <summary>
        /// Return all vertices located on a mountain, which is needed for the HillAgent
        /// </summary>
        /// <returns>The _mountainVertices list</returns>
        public List<Vector2Int> RetrieveMountainVertices()
        {
            return _mountainVertices;
        }

        /// <summary>
        /// Return the list of mountaintops, which is needed for the VolcanoAgent
        /// </summary>
        /// <returns>the _mountainTops list</returns>
        public List<Vector2Int> GetMountainTops()
        {
            return _mountainTops;
        }

    }
}
