using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleData
{
    private Vector3 x;
    private Vector3 y;
    private Vector3 z;
    private Material triangleMat;
    private Vector3[] vertices = new Vector3[3];
    private int[] indices = new int[3];

    public TriangleData(Vector3 _x, Vector3 _y, Vector3 _z, Material _triangleMat)
    {
        x = _x;
        y = _y;
        z = _z;
        triangleMat = _triangleMat;
        vertices[0] = _x;
        vertices[1] = _y;
        vertices[2] = _z;
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
    }
    public Vector3 GetX()
    {
        return x;
    }
    public Vector3 GetY()
    {
        return y;
    }
    public Vector3 GetZ()
    {
        return z;
    }
    public Material GetMaterial()
    {
        return triangleMat;
    }
    public Vector3[] GetVertices()
    {
        return vertices;
    }
    public int[] GetIndices()
    {
        return indices;
    }
}
