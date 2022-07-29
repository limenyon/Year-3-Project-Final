using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAStar
{
    public bool walkable;
    public Vector3 worldPostion;

    public NodeAStar(bool _walkable, Vector3 _worldPostion)
    {
        walkable = _walkable;
        worldPostion = _worldPostion;
    }
}
