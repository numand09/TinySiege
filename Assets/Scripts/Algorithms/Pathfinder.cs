using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{
    private GameBoardManager boardManager;
    private int maxPathfindingIterations = 5000; // Prevent infinite loops

    public Pathfinder(GameBoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    private bool IsPathClear(Vector3 start, Vector3 end)
    {
        RaycastHit2D hit = Physics2D.Raycast(start, end - start, Vector2.Distance(start, end), 1 << LayerMask.NameToLayer("Building"));
        return hit.collider == null;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 target, float z = 0f)
    {
        var startGrid = boardManager.WorldToGridPosition(start);
        var targetGrid = boardManager.WorldToGridPosition(target);

        if (!IsInBounds(startGrid) || !IsInBounds(targetGrid)) return null;

        ResetAllNodes();
        if (!boardManager.nodeGrid[targetGrid.x, targetGrid.y].IsWalkable)
            targetGrid = FindNearestWalkableNode(startGrid, targetGrid);

        var startNode = boardManager.nodeGrid[startGrid.x, startGrid.y];
        var targetNode = boardManager.nodeGrid[targetGrid.x, targetGrid.y];
        if (!targetNode.IsWalkable) return null;

        var openSet = new List<Node> { startNode };
        var closedSet = new HashSet<Node>();
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxPathfindingIterations)
        {
            iterations++;
            
            var current = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
            if (current == targetNode)
                return RetraceWorldPath(startNode, targetNode, z);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in boardManager.GetNeighbors(current))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor) || !IsPathClear(boardManager.GridToWorldPosition(current.GridPosition.x, current.GridPosition.y), boardManager.GridToWorldPosition(neighbor.GridPosition.x, neighbor.GridPosition.y)))
                    continue;

                int tentativeG = current.GCost + GetDistance(current, neighbor);
                if (tentativeG < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = tentativeG;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = current;
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }


        if (IsTargetNearBuilding(target))
        {
            Node bestNode = FindBestNodeForPartialPath(closedSet, targetNode);
            if (bestNode != null && bestNode != startNode)
            {
                return RetraceWorldPath(startNode, bestNode, z);
            }
        }

        return null;
    }

    private bool IsTargetNearBuilding(Vector3 target)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(target, 2.0f, 1 << LayerMask.NameToLayer("Building"));
        return colliders.Length > 0;
    }

    private Node FindBestNodeForPartialPath(HashSet<Node> closedSet, Node targetNode)
    {
        if (closedSet.Count == 0) return null;

        Node bestNode = null;
        int bestDistance = int.MaxValue;

        foreach (var node in closedSet)
        {
            int distance = GetDistance(node, targetNode);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestNode = node;
            }
        }

        return bestNode;
    }

    private void ResetAllNodes()
    {
        foreach (var node in boardManager.nodeGrid)
        {
            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.Parent = null;
        }
    }

    private bool IsInBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < boardManager.width && pos.y >= 0 && pos.y < boardManager.height;

    private Vector2Int FindNearestWalkableNode(Vector2Int from, Vector2Int around)
    {
        List<Vector2Int> candidates = new();
        
        for (int r = 1; r <= 7; r++)
        {
            for (int x = around.x - r; x <= around.x + r; x++)
                for (int y = around.y - r; y <= around.y + r; y++)
                    if ((Mathf.Abs(x - around.x) == r || Mathf.Abs(y - around.y) == r) &&
                        IsInBounds(new(x, y)) && boardManager.nodeGrid[x, y].IsWalkable)
                        candidates.Add(new(x, y));
            
            if (candidates.Count > 0)
                break;
        }
        
        return candidates.Count == 0 ? around : candidates.OrderBy(c => Vector2Int.Distance(from, c)).First();
    }

    private List<Vector3> RetraceWorldPath(Node start, Node end, float z)
    {
        List<Vector3> path = new();
        var current = end;
        while (current != start)
        {
            var pos = boardManager.GridToWorldPosition(current.GridPosition.x, current.GridPosition.y);
            pos.z = z;
            path.Add(pos);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dy = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        return dx > dy ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }
}