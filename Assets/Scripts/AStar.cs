using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AStar : MonoBehaviour
{
    public Text timeText;
    public Text nodesChecked;
    private System.TimeSpan time;
    public int nodesCheckedForPathing;
    public ShowNavmesh navmeshScript;
    public FindNearestUnobstructed findNearestUnobstructed;
    public GameObject player;
    public GetNeighbour getNeighbours;
    public PathTrimmer pathTrimmer;
    Vector3 startNode;
    Vector3 endNode;
    float[,] adjacencyMatrix;
    List<Vector3> vertices;
    public LineRenderer lr;
    private List<Vector3> finalPath = null;
    private int walkCounter = 0;
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            adjacencyMatrix = navmeshScript.adjacencyMatrix.GetMatrix();
            vertices = navmeshScript.vertices;
            startNode = player.transform.position;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hasHit = Physics.Raycast(ray, out hit);
            if (hasHit)
            {
                endNode = hit.point;
            }
            if (hasHit)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<int> path = RecalculatePath();
                stopwatch.Stop();
                time = stopwatch.Elapsed;
                timeText.text = "Time Taken:" + time;
                nodesChecked.text = "Nodes Check: " + nodesCheckedForPathing;
                if (path != null)
                {
                    var verticesInPath = path.Select(i => navmeshScript.meshData.vertices[i]).ToList();
                    verticesInPath.Insert(0, startNode);
                    verticesInPath.Add(endNode);
                    lr.endWidth = 0.2f;
                    lr.startWidth = 0.2f;
                    finalPath = verticesInPath;
                    lr.positionCount = finalPath.Count;
                    walkCounter = 0;
                }
            }
        }
        //draw path only if there is something in finalpath 
        if (finalPath != null)
        {
            //move and draw until the last node is reached
            if (walkCounter < finalPath.Count)
            {
                //Move the player to the target
                Move(finalPath[walkCounter], player, finalPath);
                //Draw path from player to target
                DrawPath(finalPath, walkCounter);
            }
        }
    }
    private List<int> RecalculatePath()
    {
        List<int> path = new List<int>();
        var start = findNearestUnobstructed.FindNearestIndexUnobstructed(navmeshScript.meshData.vertices, startNode);
        var end = findNearestUnobstructed.FindNearestIndexUnobstructed(navmeshScript.meshData.vertices, endNode);
        if(start == end)
        {
            return path;
        }
        if(end == -1)
        {
            return null;
        }
        path = AStarAlgorith(adjacencyMatrix, start, end);
        if (path.Count > 0)
        {
            path = pathTrimmer.TrimPath(player.transform.position, endNode, path, navmeshScript.meshData.vertices);
            return path;
        }
        return null;
    }
    List<int> AStarAlgorith(float[,] adjacencyMatrix, int start, int end)
    {
        //keeping tack of nodes that are checked through runtime
        nodesCheckedForPathing = 1;
        //getting the length of the adjacency matrix, since it's a multidimensional array
        //I need to get the square root of it's length to get the number of vertices
        int[] parent = new int[(int)Mathf.Sqrt(adjacencyMatrix.Length)];
        //similarly to Dijkstra's the parents are set to -1 to identify if they are navigatable to
        for (int p = 0; p < parent.Length; p++)
        {
            parent[p] = -1;
        }
        //A* uses the open and closed lists to check nodes that still need to be considered
        List<int> openList = new List<int>();
        //a hashset is used for set operations
        HashSet<int> closedList = new HashSet<int>();
        //ading the start to the list of nodes still to be explored
        openList.Add(start);
        //a list for the returned path
        List<int> path = new List<int>();

        //while there are nodes to explore
        while (openList.Count > 0)
        {
            //find the lowest node that is available
            //this is the most expensive part of the algorithm
            int currentNode = FindLowestCost(openList, start, end);
            if (currentNode == end)
            {
                //if we have reached our destination stop the algorithm
                break;
            }
            //remove the node that is being explored from the open list
            openList.Remove(currentNode);
            //and add it to the closed list where explored nodes reside
            closedList.Add(currentNode);

            //get the neighbours of teh currently explored node
            var neighbours = getNeighbours.GetNeighbours(adjacencyMatrix, currentNode);
            //check every single neighbour
            foreach (int neighbour in neighbours)
            {
                //add nodes to checked
                nodesCheckedForPathing++;
                //if the nodes already has neighbour simply go to the next neigbour
                if (closedList.Contains(neighbour))
                {
                    continue;
                }

                //set a comparative cost to compare if this is a valid node to explore further
                float tentativeCost = GetGCost(adjacencyMatrix, currentNode, start) + GetGCost(adjacencyMatrix, currentNode, neighbour);
                //if the tentative cost is lower than costs that already exist, this is set to be the node to follow next
                if (tentativeCost < GetGCost(adjacencyMatrix, neighbour, start) || !openList.Contains(neighbour))
                {
                    //set parent so we can trace back the path
                    parent[neighbour] = currentNode;
                    //if the node wasn't already on the list add it to the list
                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
        //set the current node to the end of the path
        int current = end;

        //check if the node is reachable
        bool reachable = parent[current] != -1;
        //if it is reachable or current is the start of the path
        if(reachable || current == start)
        {
            //while there are still nodes on the path
            while(current != -1)
            {
                //add the path node to the list
                path.Insert(0, current);
                //set the node of it's parent to be the next node to add
                current = parent[current];
            }
        }
        //remove -1 as it will be added to the list but does not exist in the vertices array
        if(path.Count > 0)
        {
            path.Remove(path[0]);
        }
        return path;
    }
    private float GetHCost(List<Vector3> vertices, int currentVertex, int endVertex)
    {
        float x = Mathf.Abs(vertices[currentVertex].x - vertices[endVertex].x);
        float y = Mathf.Abs(vertices[currentVertex].y - vertices[endVertex].y);
        float z = Mathf.Abs(vertices[currentVertex].z - vertices[endVertex].z);
        return Mathf.Sqrt(x * x + y * y + z * z);
    }
    private float GetGCost(float[,] adjacencyMatrix, int currentVertex, int startVertex)
    {
        float g = adjacencyMatrix[currentVertex, startVertex];
        return g;
    }
    private float GetFCost(float h, float g)
    {
        float f = h + g;
        return f;
    }
    private int FindLowestCost(List<int> openList, int start, int end)
    {
        int currentBest = 0;
        float currentBestValue = GetFCost(GetHCost(vertices, openList[0], end), GetGCost(adjacencyMatrix, openList[0], start));
        for(int i = 0; i < openList.Count; i++)
        {
            float newNodeValue = GetFCost(GetHCost(vertices, openList[i], end), GetGCost(adjacencyMatrix, openList[i], start));
            if (newNodeValue < currentBestValue)
            {
                currentBest = i;
                currentBestValue = newNodeValue;
            }
        }
        return openList[currentBest];
    }
    private void Move(Vector3 target, GameObject player, List<Vector3> path)
    {
        player.transform.position = Vector3.MoveTowards(player.transform.position, target, 3f * Time.deltaTime);
        if (player.transform.position == target)
        {
            walkCounter++;
        }
    }
    private void DrawPath(List<Vector3> path, int walkCounter)
    {
        var seg = path.Skip(walkCounter).Select(el => el + Vector3.up * 0.1f).ToList();
        seg.Insert(0, player.transform.position);
        lr.positionCount = seg.Count;
        lr.SetPositions(seg.ToArray());
    }
}