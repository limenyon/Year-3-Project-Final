using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAStar : MonoBehaviour
{
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    NodeAStar[,] grid;

    public float nodeDiameter;
    public int gridSizeX, gridSizeY;


    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }


    void CreateGrid()
    {
        grid = new NodeAStar[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new NodeAStar(walkable, worldPoint);
            }
        }
    }

    public NodeAStar NodeFromWorldPoint(Vector3 worldPosition)

    {

        float percentX = ((worldPosition.x - transform.position.x) / gridWorldSize.x) + .5f;
        float percentY = ((worldPosition.z - transform.position.y) / gridWorldSize.y) + .5f;


        //the +1 and +2 allow me to work out the center, depending on granularity of the nodes it might change so I will try to figure out a formula for this instead of a magic number
        //I think I did it? The 0.25f is a clamp that helps me always get a good enough result when working with doubles
        int x = Mathf.RoundToInt(Mathf.Clamp((gridSizeX) * percentX, 0, gridSizeX) + (0.25f / nodeDiameter));

        int y = Mathf.RoundToInt(Mathf.Clamp((gridSizeY) * percentY, 0, gridSizeY) + ((0.25f / nodeDiameter) * 2));

        return grid[x, y];

    }

    private void OnDrawGizmos()
    {
        //this is the thing that draws a white wirecube around the object, might wanna delete it later
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if(grid != null)
        {
            NodeAStar playerNode = NodeFromWorldPoint(player.position);
            foreach (NodeAStar n in grid)
            {
                if(n.walkable)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                if(playerNode == n)
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawCube(n.worldPostion, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
