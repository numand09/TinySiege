using System.Collections.Generic;
using UnityEngine;

public class GameBoardManager : MonoBehaviour
{
    public int width = 32;
    public int height = 32;
    [SerializeField] private float cellSize = 0.32f;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;

    private bool[,] grid;
    private Vector3 gridOrigin;
    public Node[,] nodeGrid;
    private int leftRestriction = 2;   
    private int rightRestriction = 1; 
    private int topRestriction = 3; 

    void Awake()
    {
        grid = new bool[width, height];
        nodeGrid = new Node[width, height];
        gridOrigin = cellParent.position + new Vector3(2 * cellSize, 2 * cellSize, 0f);
        ShiftGrid(2, 2);

        InitializeNodeGrid();
        
        MarkRestrictedAreas();
    }

    void Start()
    {
        InstantiateCells();
    }

    private void InitializeNodeGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                nodeGrid[x, y] = new Node(new Vector2Int(x, y), true);
    }

    private void MarkRestrictedAreas()
    {
        for (int x = 0; x < leftRestriction; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = true;

        for (int x = width - rightRestriction; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = true;

        for (int x = 0; x < width; x++)
            for (int y = height - topRestriction; y < height; y++)
                grid[x, y] = true;
    }

    private void InstantiateCells()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, cellParent);
                cell.transform.localPosition = new Vector3(x * cellSize + 0.14f, y * cellSize + 0.16f, 0f);
                
                if (IsInRestrictedArea(x, y))
                {
                    SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = new Color(1f, 0.5f, 0.5f, 0.5f);
                    }
                }
            }
    }

    private bool IsInRestrictedArea(int x, int y)
    {
        if (x < leftRestriction) return true;        
        if (x >= width - rightRestriction) return true;        
        if (y >= height - topRestriction) return true;
        
        return false;
    }

    private void ShiftGrid(int shiftX, int shiftY)
    {
        bool[,] newGrid = new bool[width, height];

        for (int x = 0; x < width - shiftX; x++)
            for (int y = 0; y < height - shiftY; y++)
                newGrid[x + shiftX, y + shiftY] = grid[x, y];

        grid = newGrid;
    }

    public bool IsAreaAvailable(int x, int y, int w, int h)
    {
        if (IsOutOfBounds(x, y, w, h))
            return false;
            
        if (IsOverlappingRestrictedArea(x, y, w, h))
            return false;

        for (int i = x; i < x + w; i++)
            for (int j = y; j < y + h; j++)
                if (grid[i, j]) return false;

        return true;
    }
    
    private bool IsOverlappingRestrictedArea(int x, int y, int w, int h)
    {
        if (x < leftRestriction && x + w > 0)
            return true;
            
        if (x + w > width - rightRestriction && x < width)
            return true;
            
        if (y + h > height - topRestriction && y < height)
            return true;
            
        return false;
    }

    private bool IsOutOfBounds(int x, int y, int w, int h)
    {
        return x < 0 || y < 0 || x + w > width || y + h > height;
    }

    public bool IsWalkable(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        return IsPositionInGrid(gridPos) && nodeGrid[gridPos.x, gridPos.y].IsWalkable;
    }

    private bool IsPositionInGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public void OccupyArea(int x, int y, int w, int h)
    {
        ProcessArea(x, y, w, h, true);
    }

    public void FreeArea(int startX, int startY, int w, int h)
    {
        ProcessArea(startX, startY, w, h, false);
    }

    private void ProcessArea(int x, int y, int w, int h, bool occupy)
{
    for (int i = x; i < x + w; i++)
        for (int j = y; j < y + h; j++)
            if (IsPositionInGrid(new Vector2Int(i, j)))
            {
                grid[i, j] = occupy;
                nodeGrid[i, j].IsWalkable = !occupy;
            }
}

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.FloorToInt(localPos.x / cellSize),
            Mathf.FloorToInt(localPos.y / cellSize)
        );
    }

    public Vector3 GridToWorldPosition(int x, int y, bool centered = true)
    {
        float offset = centered ? cellSize / 2f : 0f;
        return gridOrigin + new Vector3(x * cellSize + offset, y * cellSize + offset, 0f);
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.GridPosition.x + dx[i];
            int checkY = node.GridPosition.y + dy[i];
            
            if (IsPositionInGrid(new Vector2Int(checkX, checkY)) && nodeGrid[checkX, checkY].IsWalkable)
                neighbors.Add(nodeGrid[checkX, checkY]);
        }

        return neighbors;
    }
}