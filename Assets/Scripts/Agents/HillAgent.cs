using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

namespace Agents
{
    public class HillAgent : BaseAgent
    {
        // The lower and upper bound for the amount of hill ranges
        private readonly int _minAmountOfHills;
        private readonly int _maxAmountOfHills;

        // The lower and upper bound for the lenght of each hill range
        private readonly int _minLength;
        private readonly int _maxLength;

        // The maximum height of each hill
        private const int _maxHeight = 30;
        // The maximum variation in the height of each hill (divided by 2)
        private const int _heightVar = 10;
        // The width of each hill
        private const int _hillWidth = 50;

        // A list containing all hilltops
        private List<Vector2Int> _hillTops = new List<Vector2Int>();
        // A list containing all mountaintops
        private List<Vector2Int> _mountainTops;
        // A list containing all possible vertices for hills to spawn
        private readonly List<Vector2Int> _possibleHillPositions;
        // A list with all vertices that are located in a mountainrange
        private readonly List<Vector2Int> _mountainVertices;
        // A list with all vertices that are on the coast
        private readonly List<Vector2Int> _verticesOnCoast;

        // A random number generator used here and there
        private System.Random _rnd;

        /// <summary>
        /// Constructor of the hill agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The depth of the terrain</param>
        /// <param name="tokens">Does nothing, will be used later</param>
        /// <param name="minAmount">The minimum amount of hill chains</param>
        /// <param name="maxAmount">The maximum amount of hill chains</param>
        /// <param name="minLength">The minimum length of a hill chain</param>
        /// <param name="maxLength">The maximum length of a hill chain</param>
        public HillAgent(Point[,] vertices, int width, int depth, int tokens, int minAmount, int maxAmount, int minLength, int maxLength) : base(vertices, width, depth, tokens)
        {
            _mountainVertices = GetMountainVertices();
            _verticesOnCoast = GetCoastVertices();
            _maxLength = maxLength;
            _minLength = minLength;
            _minAmountOfHills = minAmount;
            _maxAmountOfHills = maxAmount;
            _possibleHillPositions = GeneratePossibleHillPositions();          
            _rnd = new System.Random();
        }

        /// <summary>
        /// Generate a number of hill chains
        /// </summary>
        /// <returns>The vertices of the terrain wit hhills</returns>
        public override Point[,] DoAgentJob()
        {
            // Find out how many hill chains to create
            int amountOfHills = Random.Range(_minAmountOfHills, _maxAmountOfHills);
            for (int i = 0; i < amountOfHills; i++)
            {
                Tokens = Random.Range(_minLength, _maxLength);

                GenerateHills();
            }
            // Elevate terrain around the hills
            ElevateVertices();
            
            return Vertices;
        }

        /// <summary>
        /// Generate a single chain of hills
        /// </summary>
        private void GenerateHills()
        {
            // Pick a random starting position of all the possibilities
            Vector2 position = _possibleHillPositions[Random.Range(0, _possibleHillPositions.Count)];
            _possibleHillPositions.Remove(new Vector2Int((int)position.x, (int)position.y));         
            Vector2Int intPosition = new Vector2Int((int)position.x,(int)position.y);
            // Stop this chain if the initial position is too close to the coast
            Vector2Int closestCoast = GetClosestCoast(intPosition);
            if (closestCoast.x <= 1 || closestCoast.y <= 1)
            {
                return;
            }
            // Move away from the coast
            Vector2 initialDirection = intPosition - closestCoast;
            initialDirection.Normalize();           
            // Introduce more randomness to the initial direction so it will go at most perpendicular to the closest coast vector
            Vector2 newDirection = GetDirectionInAngle(initialDirection, 90);
            for (var i = 0; i < Tokens; i++)
            {
                // Change direction every 50 tokens
                if (i % 50 == 0) newDirection = GetDirectionInAngle(initialDirection, 90);
                position += newDirection;
                intPosition = new Vector2Int((int)position.x, (int)position.y);
                var vertex = Vertices[intPosition.x, intPosition.y].Vertex;
                // Return if the vertex is below sealevel
                if (vertex.y == 0)
                {
                    return;
                }            
                // Increase the height of the vertex to a number between the max height, and the max height minus the bound
                float height = _maxHeight - (float)_rnd.NextDouble() * _heightVar;
                Vertices[intPosition.x, intPosition.y].Vertex = new Vector3(vertex.x, height, vertex.z);                
                if (i % 20 == 0)
                {      
                    _hillTops.Add(intPosition);
                }
            }
        }

        /// <summary>
        /// Elevate all vertices near newly created hilltops
        /// </summary>
        private void ElevateVertices()
        {
            // Check each vertex individually how much it must be raised, depending on the mountain width and distance to mountaintops
            foreach (Point point in Vertices)
            {
                Vector3 vertex = point.Vertex;
                Vector2Int position = new Vector2Int((int)vertex.x, (int)vertex.z);
                // Skip if the vertex is underwater
                if (vertex.y < 1)
                {
                    continue;
                }
                
                // Remember all mountain and hilltops that are in range
                List<Vector2Int> closestHill = new List<Vector2Int>();
                List<float> distances = new List<float>();
                List<float> normalizedDistances = new List<float>();
                
                foreach (Vector2Int hillTop in _hillTops)
                {
                    // Calculate the distance between the mountaintop and the vertex
                    float distance = Vector2Int.Distance(position, hillTop);
                    // IF the distance is shorter than the width add it to the list
                    if (distance < _hillWidth)
                    {
                        closestHill.Add(hillTop);
                        distances.Add(distance);
                    }
                }
                // Skip if no hilltops are nearby
                if (closestHill.Count == 0) continue;
                foreach (Vector2Int mountainTop in _mountainTops)
                {
                    // Calculate the distance between the mountaintop and the vertex
                    float distance = Vector2Int.Distance(position, mountainTop);
                    // IF the distance is shorter than the width add it to the list
                    if (distance < _hillWidth)
                    {
                        closestHill.Add(mountainTop);
                        distances.Add(distance);
                    }
                }
                // Normalize the list of distances after inverting them, so that mountains that are close by have more influence
                float sumList = 0;
                for (int i = 0; i < distances.Count; i++)
                {
                    float reverseDistance = _hillWidth - distances[i];
                    sumList += reverseDistance;
                    normalizedDistances.Add(_hillWidth - distances[i]);
                }
                for (int i = 0; i < distances.Count; i++)
                {
                    normalizedDistances[i] *= 1 / sumList;
                }
                // Calculate the total height of all nearby hill and mountaintops
                float totalHeight = 0;
                Debug.Log(normalizedDistances.Sum());
                for (int i = 0; i < closestHill.Count; i++)
                {
                    float mountainHeight = Vertices[closestHill[i].x, closestHill[i].y].Vertex.y;
                    totalHeight += (mountainHeight - distances[i] * (mountainHeight / _hillWidth)) * normalizedDistances[i];
                }
                // Only replace the heights if the new height is larger
                if (totalHeight > vertex.y)
                {
                    Vertices[position.x, position.y] = new Point(new Vector3(position.x,totalHeight, position.y));
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
        /// Return a list of all possible spawning positions for hills
        /// </summary>
        /// <returns>A list with possible locations for hills</returns>
        private List<Vector2Int> GeneratePossibleHillPositions()
        {
            var possibleHillPositions = new List<Vector2Int>();
            
            foreach (var vertexPos in _mountainVertices)
            {
                if (Vertices[vertexPos.x, vertexPos.y].Vertex.y <= 10)
                {
                    possibleHillPositions.Add(vertexPos);
                }
            }
            
            return possibleHillPositions;
        }

        /// <summary>
        /// Get a list with all mountaintops created by the Mountain Agent
        /// </summary>
        /// <param name="MountainTops">A list with all mountaintops</param>
        public void GetMountainTops(List<Vector2Int> MountainTops)
        {
            _mountainTops = MountainTops;
        }
    }
}
