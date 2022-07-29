using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacencyMatrix
{
    float[,] adjacencyMatrix;
    public AdjacencyMatrix(int size)
    {
        //the constructor with the passed down size
        adjacencyMatrix = new float[size, size];
    }
    public float[,] GetMatrix()
    {
        //simple return
        return adjacencyMatrix;
    }
    public void SetAdjacency(int posX, int posY, float distance)
    {
        //set adjacency from vector PosX and vector PosY
        adjacencyMatrix[posX, posY] = distance;
        //set a backwards vector from PosY to PosX because edges go both ways
        adjacencyMatrix[posY, posX] = distance;
    }
    public float length()
    {
        //The length returned is the full X*X size so it must be square rooted
        return Mathf.Sqrt(adjacencyMatrix.Length);
    }
}