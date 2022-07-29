using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics;

public class Dijkstra : MonoBehaviour
{
    public Text timeText;
    public Text nodesChecked;
    private System.TimeSpan time;
    public int nodesCheckedForPathing;
    public GameObject player;
    public ShowNavmesh navmeshScript;
    public LineRenderer lr;
    public PathTrimmer pathTrimmer;
    Vector3 startNode;
    Vector3 endNode;
    public FindNearestUnobstructed findNearestUnobstructed;
    public GetNeighbour getNeighbours;
    private List<Vector3> finalPath = null;
    private int walkCounter = 0;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startNode = player.transform.position;
            //cast out a ray on mouse click towards the mouse position in 3D space
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //a variable used to store hit data
            RaycastHit hit;
            //cast out the ray and return a bool
            bool hasHit = Physics.Raycast(ray, out hit);
            //set the goal to be the point that was hit
            if (hasHit)
            {
                endNode = hit.point;
            }
            if(hasHit)
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
        if(finalPath != null)
        {
            if(walkCounter < finalPath.Count)
            {
                Move(finalPath[walkCounter], player, finalPath);
                DrawPath(finalPath, walkCounter);
            }
        }
    }

    private List<int> RecalculatePath()
    {
        List<int> path = new List<int>();
        var start = findNearestUnobstructed.FindNearestIndexUnobstructed(navmeshScript.meshData.vertices, player.transform.position);
        var end = findNearestUnobstructed.FindNearestIndexUnobstructed(navmeshScript.meshData.vertices, endNode);
        if (start == end)
        {
            return path;
        }
        if (end == -1)
            {
                return null;
            }
        path = DijkstraAlgorithm(navmeshScript.adjacencyMatrix.GetMatrix(), start, end);
        if (path.Count > 0)
        {
            path = pathTrimmer.TrimPath(player.transform.position, endNode, path, navmeshScript.meshData.vertices);
            return path;
        }
        return null;
    }

    //Inneficienct algorithm used for pathfinding through brute force
    private List<int> DijkstraAlgorithm(float[,] edges, int start, int goal)
    {
        //counter to see how many nodes are checked in total
        //important for understanding why the algorithm took as long as it did
        nodesCheckedForPathing = 1;
        //a safety measure for fringe cases
        int unreachableSafety = 0;
        //edges is the adjacency matrix created a while back
        int n = edges.GetLength(0);
        //distances is used to represent what the distances between neighbours is
        float[] distance = new float[n];
        //previous tells what node came previously so we could backtrack it for our full path
        int[] previous = new int[n];

        //a list of nodes that still need to be explored
        //By having this list I can ignore nodes that I have already checked
        HashSet<int> unvisited = new HashSet<int>();

        //setting the initial values
        for (int i = 0; i < n; i++)
        {
            //start off with all distances being max value
            //this way they cannot get in the way of our search for the smallest node
            distance[i] = float.MaxValue;
            //previous is set to -1 by default to have a replacable value
            previous[i] = -1;
            //add all vertices to the unvisited list as none of them have been visited
            unvisited.Add(i);
        }
        //set the distance of the start position to 0
        //this way it will always start as the lowest and that's how we find our start node
        distance[start] = 0;
        //do this until there are unviisted nodes
        while (unvisited.Count > 0)
        {
            //break out of loop in fringe cases where the algorithm infinitely loops
            if (unreachableSafety > n)
            {
                break;
            }
            //find the minimum distance that is in the distances array
            int u = MinimumDistance(distance, unvisited, n);
            //if distances is our goal we can stop the algorithm
            if (u == goal)
            {
                break;
            }
            //remove the vertex from the unvisited list as we have visited it
            unvisited.Remove(u);

            //get the neighbours of the node being currently explored
            var neighbours = getNeighbours.GetNeighbours(edges, u);
            //get the neighbours that have not been visited yet
            var unvisitedNeighbours = neighbours.Where(v => unvisited.Contains(v));
            //check every unvisited neighbour
            foreach (var v in neighbours)
            {
                //each neighbour checked is another node added to the list
                nodesCheckedForPathing++;
                //variable used to store the value of this neighbour
                var alt = distance[u] + edges[u, v];
                //checking if the value above is less than the alternatives, checking that the neighbour is not visited already
                //and that there actually is a connection
                if (unvisited.Contains(v) && alt < distance[v] && distance[u] != float.MaxValue)
                {
                    //set the distance between vertex and it's neighbour
                    distance[v] = alt;
                    //set neighbour previous indicator to the neighbour
                    previous[v] = u;
                }
            }
            //add to the safety switch
            unreachableSafety++;
        }
        //returning a list of indices that are used for creating the path
        List<int> path = new List<int>();
        //setting the first node to back track from to be our goal
        int current = goal;

        //if the previous node to the current node is still -1 that means there was no path to the part of the mesh
        bool reachable = previous[current] != -1;
        //checking if the node is reachable or if the node is the start ndoe
        if (reachable || current == start)
        {
            //while there are still nodes on the path
            while (current != -1)
            {
                //insert the node to the location 0 of the path list, pushing back other existing parts of path
                path.Insert(0, current);
                //set the node being current explored to the previous node in the path
                current = previous[current];
            }
        }
        //finally return the list of indices
        return path;
    }
    private static int MinimumDistance(float[] distance, HashSet<int> unvisited, int n)
    {
        float min = int.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < n; ++v)
        {
            if (unvisited.Contains(v) && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }
        return minIndex;
    }
    private void Move(Vector3 target, GameObject player, List<Vector3> path)
    {
        player.transform.position = Vector3.MoveTowards(player.transform.position, target, 3f * Time.deltaTime);
        if(player.transform.position == target)
        {
            walkCounter++;
        }
    }
    //used to display the visual of what the path is using line renderer
    private void DrawPath(List<Vector3> path, int walkCounter)
    {
        //Ignore nodes that we have already been to
        //Walk counter simply allows the check of how many of the vertices have been reached
        //And we do not need to see nodes that have been went to
        //Only need to see nodes that are still on the path
        var seg = path.Skip(walkCounter).Select(el => el + Vector3.up * 0.1f).ToList();
        //insert the player location to the very start so we can draw the line from the player's position
        seg.Insert(0, player.transform.position);
        //Set the amount of vertices linerenderer will hold
        lr.positionCount = seg.Count;
        //add the parts of the path that still remain
        lr.SetPositions(seg.ToArray());
    }
}