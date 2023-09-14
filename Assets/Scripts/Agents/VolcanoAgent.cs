using System.Collections.Generic;
using System;
using UnityEngine;

namespace Agents
{
    public class VolcanoAgent : BaseAgent
    {
        // The set width of the caldera
        private int _calderaWidth;
        // The maximum variation in width of the caldera (divided by 2)
        private float _calderaWidthRange;
        // The maximum height of the volcano
        private int _volcanoHeight;
        // The maximum variation in height of the volcano (divided by 2)
        private float _volcanoHeightRange;
        // The width of the volcano
        private int _volcanoWidth;
        // The maximum amount of noise on the slope of the volcano
        private float _noise = 0.2f;
        // A list of all vertices on the coast
        private List<Vector2Int> _verticesOnCoast;
        // A list of all mountaintops
        private List<Vector2Int> _mountainTops;
        // A list of all volcano centers
        private List<Vector2Int> _volcanoCenters;
        // A list of calderawidths for each volcano
        private List<float> _calderaWidths;
        // Random number generator used here and there
        private System.Random _rnd;

        /// <summary>
        /// Constructor for the volcano agent
        /// </summary>
        /// <param name="vertices">The vertices of the terrain</param>
        /// <param name="width">The width of the terrain</param>
        /// <param name="depth">The height of the terrain</param>
        /// <param name="tokens">The amount of volcanoes</param>
        /// <param name="mountainTops">A list containing all mountain tops</param>
        /// <param name="vcWidth">The width of the caldera</param>
        /// <param name="cWidthRange">The maximum variation in width of the caldera (divided by 2 </param>
        /// <param name="vHeight">The height of the volcano</param>
        /// <param name="vHeightRange">The maximum variation in height of the volcano</param>
        /// <param name="vWidth">The width of the volcano</param>
        public VolcanoAgent(Point[,] vertices, int width, int depth, int tokens, List<Vector2Int> mountainTops, int vcWidth, float cWidthRange, int vHeight, float vHeightRange, int vWidth) : base(vertices, width, depth, tokens)
        {
            this._mountainTops = mountainTops;
            _calderaWidth = vcWidth;
            _calderaWidthRange = cWidthRange;
            _volcanoHeight = vHeight;
            _volcanoHeightRange = vHeightRange;
            _volcanoWidth = vWidth;

            _volcanoCenters = new List<Vector2Int>();
            _calderaWidths = new List<float>();
            _rnd = new System.Random();
        }

        /// <summary>
        /// Generate a number of volcanoes
        /// </summary>
        /// <returns>The vertices of the terrain with volcanoes</returns>
        public override Point[,] DoAgentJob()
        {
            // Generate a number of volcanoes equal to the tokens
            for (int j = 0; j < Tokens; j++)
            {
                // Stop if you are out of mountaintops
                if (_mountainTops.Count == 0) break;
                // Pick a random mountain top for the volcano
                Vector2Int volcanoCenter = _mountainTops[_rnd.Next(_mountainTops.Count)];
                _volcanoCenters.Add(volcanoCenter);
                _mountainTops.Remove(volcanoCenter);
                // Set the volcano vertex to a random height
                float height = _volcanoHeight + ((float)_rnd.NextDouble() * _volcanoHeightRange) - _volcanoHeightRange / 2;
                float slope = height / _volcanoWidth;
                Vertices[volcanoCenter.x, volcanoCenter.y].Vertex = new Vector3(volcanoCenter.x, height, volcanoCenter.y);
                // Adjust the height on all vertices if they are close enough to the volcano
                float calderaRadius = _calderaWidth + (float)_rnd.NextDouble() * _calderaWidthRange - (_calderaWidthRange / 2);
                _calderaWidths.Add(calderaRadius);
                foreach (Point p in Vertices)
                {
                    Vector3 vertex = p.Vertex;
                    // Skip vertices that are under water
                    if (vertex.y == 0) continue;
                    // Skip vertices that are too far from the volcano
                    float distance = Vector2Int.Distance(new Vector2Int((int)vertex.x, (int)vertex.z), volcanoCenter);
                    if (distance > _volcanoWidth) continue;
                    // If the vertex is close enough to the center, set the height to 2
                    else if (distance < calderaRadius)
                    {
                        Vertices[(int)vertex.x, (int)vertex.z].Vertex.y = 2;
                        continue;
                    }
                    // Otherwise, find all mountaintops in range
                    List<Vector2Int> closestMountains = new List<Vector2Int>();
                    List<float> distances = new List<float>();
                    List<float> normalizedDistances = new List<float>();
                    foreach (Vector2Int mountainTop in _mountainTops)
                    {
                        // Calculate the distance between the mountaintop and the vertex
                        float mountainDistance = Vector2Int.Distance(new Vector2Int((int)vertex.x, (int)vertex.z), mountainTop);
                        // If the distance is shorter than the width add it to the list
                        if (mountainDistance < _volcanoWidth)
                        {
                            closestMountains.Add(mountainTop);
                            distances.Add(mountainDistance);
                        }
                    }
                    // Normalize the list of distances after inverting them, so that mountains that are close by have more influence
                    float sumList = 0;
                    for (int i = 0; i < distances.Count; i++)
                    {
                        float reverseDistance = _volcanoWidth - distances[i];
                        sumList += reverseDistance;
                        normalizedDistances.Add(_volcanoWidth - distances[i]);
                    }
                    for (int i = 0; i < normalizedDistances.Count; i++)
                    {
                        normalizedDistances[i] *= 1 / sumList;
                    }
                    // Calcualte the weighted height of all mountains
                    float totalHeight = 0;
                    for (int i = 0; i < closestMountains.Count; i++)
                    {
                        float mountainHeight = Vertices[closestMountains[i].x, closestMountains[i].y].Vertex.y;
                        totalHeight += (mountainHeight - distances[i] * (mountainHeight / _volcanoWidth)) * normalizedDistances[i];
                    }
                    // The total height of the volcano is the combined height of all neighbouring mountains and the volcano, influenced by the distance to them
                    float newHeight = (((totalHeight * distance) + (height - (slope * distance)) * (_volcanoWidth - distance)) / _volcanoWidth) + (float)_rnd.NextDouble() * _noise - _noise / 2;
                    // Replace the old height of the vertex if this is higher
                    if (newHeight > vertex.y)
                    {
                        Vertices[(int)vertex.x, (int)vertex.z].Vertex = new Vector3((int)vertex.x, newHeight, (int)vertex.z);
                    }
                }
            }
            return Vertices;
        }

        /// <summary>
        /// Return the list of volcanoes, needed for the lava agent
        /// </summary>
        /// <returns>The coordinates of the centers of all volcanoes</returns>
        public List<Vector2Int> GetVolcanoes()
        {
            return _volcanoCenters;
        }
        
        /// <summary>
        /// Return the list of caldera widths, needed for the lava agent
        /// </summary>
        /// <returns>The caldera widths of all volcanoes</returns>
        public List<float> GetCalderaWidths()
        {
            return _calderaWidths;
        }
    }
}
