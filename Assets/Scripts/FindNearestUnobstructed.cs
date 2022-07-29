using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindNearestUnobstructed : MonoBehaviour
{
    public int FindNearestIndexUnobstructed(Vector3[] vertices, Vector3 position)
    {
        //start off with -1 so all nodes start at the same value
        int nearestIndex = -1;
        //the lowest distance starts at the highest number possible as to get instantly replaced if there's any alternative at all
        float nearestDistance = float.MaxValue;
        //look through every vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            //using unity's native distance finder find the distance between the vertex in the vertices array and start(position)
            float distance = Vector3.Distance(vertices[i], position);
            //compare the distance with the current nearest distance
            if (distance < nearestDistance)
            {
                //check if there are any objects that would obstruct the direct path from one vertex to the other
                if (!NavMesh.Raycast(vertices[i], position, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    //we have found our new closest vector and we store it
                    nearestIndex = i;
                    //we store the distance for further comparisons
                    nearestDistance = distance;
                }
            }
        }
        //return the index of the vertex array that has the nearest option
        return nearestIndex;
    }
}