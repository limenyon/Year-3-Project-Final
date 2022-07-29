using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetNeighbour : MonoBehaviour
{
    public int[] GetNeighbours(float[,] matrix, int row)
    {
        return Enumerable
            .Range(0, matrix.GetLength(1))
            .Where(col => matrix[row, col] > 0)
            .ToArray();
    }
}