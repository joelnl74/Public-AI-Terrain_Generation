using Agents;
using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class TerrainManager : Singleton<TerrainManager>
{
    // Width of our quad.
    private const int Width = 1024;
    // Depth of our plane.
    private const int Depth = 1024;
    
    // An array of all vertices, sorted by their X and Z coördinates
    [HideInInspector] public Point[,] Vertices;
    // A list of land vertices that are adjecent to water
    [HideInInspector] public List<Vector2Int> VerticesOnCoast;
    // A list of vertices that are located on mountainous terrain
    [HideInInspector] public List<Vector2Int> MountainVertices;

    // References to the mesh filter and gradient.
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Gradient _gradient;

    // The terrain mesh
    private Mesh _mesh;
    // The triangles of the terrain mesh
    private int[] _triangles;

    private float _minHeight;
    private float _maxHeight;


    // All of the individual agents used
    private CoastAgent _coastAgent;

    private NoiseAgent _noiseAgent1;
    private NoiseAgent _noiseAgent2;

    private SmoothingAgent _smoothingAgent1;
    private SmoothingAgent _smoothingAgent2;

    private MountainAgent _mountainAgent;
    private HillAgent _hillAgent;
    private BeachAgent _beachAgent;

    private VolcanoAgent _volcanoAgent;
    private LavaAgent _lavaAgent;


    public void GenerateTerrain(SettingsModel settingsModel)
    {
        InitMesh();

        if (settingsModel.OneIsland)
        {
            // Start off by creating the coast
            _coastAgent = new CoastAgent(Vertices, Width, Depth, Width * Depth / 3, settingsModel.borderSize);
            Vertices = _coastAgent.DoAgentJob();
            VerticesOnCoast = _coastAgent.RetrieveCoastVertices();

            // Add a small layer of smoothened noise to make the terrain less flat
            _noiseAgent1 = new NoiseAgent(Vertices, Width, Depth, 1, 1, 20, 30);
            Vertices = _noiseAgent1.DoAgentJob();

            _smoothingAgent1 = new SmoothingAgent(Vertices, Width, Depth, 3);
            Vertices = _smoothingAgent1.DoAgentJob();

            // Fill the terrain with mountains and hills
            _mountainAgent = new MountainAgent(Vertices, Width, Depth, 200, settingsModel.maxHeight, settingsModel.mountainWidth, settingsModel.minLength, settingsModel.maxLength, settingsModel.minAmountOfMountains, settingsModel.maxAmountOfMountains);
            Vertices = _mountainAgent.DoAgentJob();
            MountainVertices = _mountainAgent.RetrieveMountainVertices();   
            
            _hillAgent = new HillAgent(Vertices, Width, Depth, 100, settingsModel.minAmountOfHill, settingsModel.maxAmountOfHill, settingsModel.minHillLength, settingsModel.minHillLength);
            _hillAgent.GetMountainTops(_mountainAgent.GetMountainTops());
            Vertices = _hillAgent.DoAgentJob();          
            
            // Smoothen the mountains and hills to improve their slopes.
            _smoothingAgent2 = new SmoothingAgent(Vertices, Width, Depth, 8);
            Vertices = _smoothingAgent2.DoAgentJob();

            // Create the beaches
            _beachAgent = new BeachAgent(Vertices, Width, Depth, 200, settingsModel.numberOfBeaches, settingsModel.inlandDistance, settingsModel.beachSealevel, settingsModel.beachMaxHeight);
            Vertices = _beachAgent.DoAgentJob();
            
            // Smoothen the beaches to improve the transitions between beaches and the higher inland terrain
            Vertices = new SmoothingAgent(Vertices, Width, Depth, 2).DoAgentJob();

            // Adding a small amount of noise to make everything slightly less smooth
            Vertices = new NoiseAgent(Vertices, Width, Depth, 1, 10, 0.1f, 0.1f).DoAgentJob();
            //Vertices = new SmoothingAgent(Vertices, Width, Depth, 1).DoAgentJob();
            
            // Create a number of volcanoes
            _volcanoAgent = new VolcanoAgent(Vertices, Width, Depth, 3, _mountainAgent.GetMountainTops(), settingsModel.calderaWidth, settingsModel.calderaWidthRange, settingsModel.volcanoHeight, settingsModel.volcanoHeightRange, settingsModel.volcanoWidth);
            Vertices = _volcanoAgent.DoAgentJob();

            // Create a number of lava rivers originating from the volcanoes, max 1 river per volcano
            _lavaAgent = new LavaAgent(Vertices, Width, Depth, 2, _volcanoAgent.GetVolcanoes(), _volcanoAgent.GetCalderaWidths());
            Vertices = _lavaAgent.DoAgentJob();
        }

        UpdateMesh();
    }

    private void InitMesh()
    {
        _mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        
        _meshFilter.mesh = _mesh;
        
        Vertices = new Point[Width + 1, Depth + 1];

        for (var z = 0; z <= Depth; z++)        
        {
            for (var x = 0; x <= Width; x++)
            {
                Vertices[x, z] = new Point(new Vector3(x, 0, z));
            }
        }
        
        BuildTriangles();
    }

    private void BuildTriangles()
    {
        _triangles = new int[Width * Depth * 6];
        var vert = 0;
        var tris = 0;

        for (var z = 0; z < Depth; z++)
        {
            for (var x = 0; x < Width; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + Width + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + Width + 1;
                _triangles[tris + 5] = vert + Width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }
    private void UpdateMesh()
    {
        var vertices = new Vector3[(Width + 1) * (Depth + 1)];
        var colors = new Color[(Width + 1) * (Depth + 1)];
        
        for (int i = 0, z = 0; z <= Depth; z++)        
        {
            for (var x = 0; x <= Width; x++)
            {
                var vertex = Vertices[x, z].Vertex;
                
                vertices[i] = new Vector3(vertex.x, vertex.y, vertex.z);

                if (vertex.y < _minHeight)
                {
                    _minHeight = vertex.y;
                }

                if (vertex.y > _maxHeight)
                {
                    _maxHeight = vertex.y;
                }

                i++;
            }
        }
        
        for (int i = 0, z = 0; z <= Depth; z++)        
        {
            for (var x = 0; x <= Width; x++)
            {
                var point = Vertices[x, z];
                var vertex = point.Vertex;
                var color = point.Color;

                if (color == Color.magenta)
                {
                    colors[i] = _gradient.Evaluate(Mathf.InverseLerp(_minHeight, _maxHeight, vertex.y));
                }
                else
                {
                    colors[i] = color;
                }

                i++;
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = colors;

        _mesh.RecalculateNormals();
    }
}
