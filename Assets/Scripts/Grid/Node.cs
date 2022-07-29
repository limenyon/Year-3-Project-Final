using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool walkable;
    public Vector3 position;

    public Node(bool _walkable, Vector3 _position)
    {
        walkable = _walkable;
        position = _position;
    }
}
