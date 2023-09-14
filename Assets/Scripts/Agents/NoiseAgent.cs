using UnityEngine;

namespace Agents
{
    public class NoiseAgent : BaseAgent
    {
        // The chance per vertex that noise will be increased
        private readonly int _randomNoiseGenerationPercentage;

        // The lower and uppper bound for the noise values
        private readonly float _randomNoiseMinHeight;
        private readonly float _randomNoiseMaxHeight;
        
        /// <summary>
        /// Constructor of the noise agent
        /// </summary>
        /// <param name="vertices">List of vertices of the terrain</param>
        /// <param name="width">Width of the terrain</param>
        /// <param name="depth">Depth of the terrain</param>
        /// <param name="tokens">Number of iterations of the noising algorithm</param>
        /// <param name="chance">Chance of noise per vertex</param>
        /// <param name="minHeight">Minimum height increase for noise</param>
        /// <param name="maxHeight">Maximum height increase for noise</param>
        public NoiseAgent(Point[,] vertices, int width, int depth, int tokens, int chance, float minHeight, float maxHeight) : base(vertices, width, depth, tokens)
        {
            _randomNoiseMinHeight = minHeight;
            _randomNoiseMaxHeight = maxHeight;

            _randomNoiseGenerationPercentage = chance;
        }

        /// <summary>
        /// Add noise to the terrain
        /// </summary>
        /// <returns>The vertices of the terrain with noise</returns>
        public override Point[,] DoAgentJob()
        {
            for (var i = 0; i < Tokens; i++)
            {
                foreach (Point point in Vertices)
                {
                    Vector3 vertex = point.Vertex;
                    // Skip vertices that are underwater
                    if (vertex.y < 1) continue;
                    // Check whether or not to add noise, based on a random value
                    var changeDirectionChance = Random.Range(0, 100);
                    // Skip vertices that do not have noise added
                    if (_randomNoiseGenerationPercentage < changeDirectionChance)
                    {
                        continue;
                    }
                    // Otherwise add the noise on top of the existing y values of the vertex
                    var heightChange = Random.Range(_randomNoiseMinHeight, _randomNoiseMaxHeight);
                    Vertices[(int)vertex.x, (int)vertex.z] = new Point(new Vector3((int)vertex.x, vertex.y += heightChange, (int)vertex.z));
                }
            }      
            return Vertices;
        }
    }
}
