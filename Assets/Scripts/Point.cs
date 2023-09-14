using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Point(Vector3 vertex)
    {
        Vertex = vertex;
    }
    
    public Vector3 Vertex;
    public Color Color = Color.magenta;
}
