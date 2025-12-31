using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public class Node
    {
        public Vector2Int position;
        public Vector3 worldPosition; // posición en mundo (centro de la celda)
        public bool isWalkable;

        public int gCost; // distancia desde el inicio
        public int hCost; // estimación hasta el final
        public int fCost => gCost + hCost;

        public Node parent; // para reconstruir el camino
    }

    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero; // esquina inferior-izquierda de la grilla

    [Header("Debug / Visualization")]
    public bool visualizeGrid = true;
    [Range(0.1f, 1f)] public float gizmoScale = 0.9f;

    private Node[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    // Permite generar la grilla desde el menú del componente en el Inspector
    [ContextMenu("Generate Grid")]
    public void GenerateGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(
                    gridOrigin.x + x * cellSize + cellSize * 0.5f,
                    gridOrigin.y,
                    gridOrigin.z + y * cellSize + cellSize * 0.5f
                );

                grid[x, y] = new Node
                {
                    position = new Vector2Int(x, y),
                    worldPosition = worldPos,
                    isWalkable = true
                };
            }
        }
    }

    public List<Node> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        if (grid == null) GenerateGrid();

        if (!IsInsideGrid(startPos) || !IsInsideGrid(endPos))
        {
            Debug.LogWarning($"Pathfinding: start {startPos} or end {endPos} outside grid bounds.");
            return null;
        }

        Node startNode = grid[startPos.x, startPos.y];
        Node endNode = grid[endPos.x, endPos.y];

        List<Node> openList = new List<Node> { startNode };
        HashSet<Node> closedList = new HashSet<Node>();

        foreach (Node node in grid)
        {
            node.gCost = int.MaxValue;
            node.parent = null;
        }

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, endNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedList.Contains(neighbor)) continue;

                int newCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; // no hay camino
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.position.x + x;
                int checkY = node.position.y + y;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.position.x - b.position.x);
        int dstY = Mathf.Abs(a.position.y - b.position.y);
        return 10 * (dstX + dstY);
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Add(startNode);
        path.Reverse();
        return path;
    }


    [Header("Debug")]
    public Vector2Int startPosition;
    public Vector2Int endPosition;
    private List<Node> debugPath;

    void OnDrawGizmos()
    {
        if (!visualizeGrid) return;

        if (grid == null)
        {
            // intenta generar para visualizar (solo en editor)
            #if UNITY_EDITOR
            GenerateGrid();
            #endif
        }

        if (grid == null) return;

        // dibuja celdas
        foreach (Node node in grid)
        {
            Gizmos.color = node.isWalkable ? Color.white : Color.black;

            if (debugPath != null && debugPath.Contains(node)) Gizmos.color = Color.green;
            if (node.position == startPosition) Gizmos.color = Color.blue;
            if (node.position == endPosition) Gizmos.color = Color.red;

            Vector3 size = Vector3.one * (cellSize * gizmoScale);
            Gizmos.DrawWireCube(node.worldPosition, size);
        }

        // dibuja línea del path (si existe)
        if (debugPath != null && debugPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < debugPath.Count; i++)
            {
                Gizmos.DrawSphere(debugPath[i].worldPosition, cellSize * 0.15f);
                if (i + 1 < debugPath.Count)
                    Gizmos.DrawLine(debugPath[i].worldPosition, debugPath[i + 1].worldPosition);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            debugPath = FindPath(startPosition, endPosition);
        }
    }

    public bool IsInsideGrid(Vector2Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

    // convierte world->grid index usando gridOrigin y cellSize
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.z - gridOrigin.z) / cellSize);
        return new Vector2Int(x, y);
    }

    // convierte grid index -> world (centro de celda)
    public Vector3 GridToWorld(Vector2Int gridPos, float y = 0f)
    {
        return new Vector3(
            gridOrigin.x + gridPos.x * cellSize + cellSize * 0.5f,
            y,
            gridOrigin.z + gridPos.y * cellSize + cellSize * 0.5f
        );
    }
}