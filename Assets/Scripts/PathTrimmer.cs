using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathTrimmer : MonoBehaviour
{
    private NavMeshHit hit;
    //used for trimming down the path to prevent walking in wrong direction
    //and allow for direct targetting between start and end
    public List<int> TrimPath(Vector3 start, Vector3 end, List<int> path, Vector3[] vertices)
    {
        //we store nodes to remove rather than remove as soon as we find them
        //because this way the list remains unchanged which is what is needed for a successful run
        List<int> nodesToRemove = new List<int>();
        //loop through the path
        for(int i = 0; i < path.Count; i++)
        {
            //check if there is nothing in the way between the two vertices
            if (!NavMesh.Raycast(start, vertices[path[i]], out hit, NavMesh.AllAreas))
            {
                nodesToRemove.Add(path[i]);
            }
            //at the end of the loop we have to remove the last node we added to the remove list
            //because it will be the last node that we can navigate to directly
            else if(nodesToRemove.Count > 0)
            {
                //break out
                nodesToRemove.Remove(nodesToRemove[nodesToRemove.Count - 1]);
                break;
            }
            else
            {
                //breaking out instantly if there is something in the way
                //as we now have our array of unobstructed vertices
                break;
            }
        }
        //repeat what was done before but from the end node
        //this way we can trim nodes from both sides to minimize the amount of nodes on the path
        List<int> nodesToRemoveBack = new List<int>();
        if(path.Count > 0)
        {
            for(int j = path.Count; j > 0; j--)
            {
                if (!NavMesh.Raycast(end, vertices[path[j - 1]], out hit, NavMesh.AllAreas))
                {
                    nodesToRemoveBack.Add(path[j - 1]);
                }
                else if(nodesToRemoveBack.Count > 0)
                {
                    nodesToRemoveBack.Remove(nodesToRemoveBack[nodesToRemoveBack.Count - 1]);
                    break;
                }
                else
                {
                    break;
                }
            }
            //These have to happen after the checks otherwise there are possible scenarios where the path goes through walls
            foreach(int pathToRemove in nodesToRemove)
            {
                path.Remove(pathToRemove);
            }
            foreach(int pathToRemove in nodesToRemoveBack)
            {
                path.Remove(pathToRemove);
            }
        }
        return path;
    }
}