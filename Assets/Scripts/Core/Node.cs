using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int GridPosition;
    public bool IsWalkable;
    public int GCost;
    public int HCost;
    public Node Parent;

    public int FCost => GCost + HCost;

    public Node(Vector2Int position, bool isWalkable)
    {
        GridPosition = position;
        IsWalkable = isWalkable;
    }
}
