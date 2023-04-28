using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem instance;
    public static List<Vector3> points;

    [SerializeField]
    private Position posPrefab;
    [SerializeField]
    private GameObject gate;
    [SerializeField]
    private GameObject king;

    private int numCols = 15;
    private int numRows = 7;
    private List<Position> positions;
    private Vector2 centerPos;
    private Vector2Int startPos;
    private Vector2Int endPos;

    private readonly int[,] map = {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1 },
        { 2, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 3 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };

    private readonly Vector2Int[] directions = { Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.left };

    private void Awake()
    {
        instance = this;

        positions = new List<Position>();
        points = new List<Vector3>();

        centerPos = new Vector2(Mathf.FloorToInt(numCols / 2f), Mathf.FloorToInt(numRows / 2f));

        GenerateMap();
        GenerateWaypoints();
    }

    private void GenerateMap()
    {
        for (int y = 0; y < numRows; y++)
        {
            for (int x = 0; x < numCols; x++)
            {
                // Đường đi
                if (map[y, x] == 0)
                {
                    continue;
                }

                // Block (position)
                if (map[y, x] == 1)
                {
                    Position pos = Instantiate(posPrefab, transform);
                    pos.SetValue(new Vector2(x, y), centerPos);
                    positions.Add(pos);
                    continue;
                }

                // Start pos
                if (map[y,x] == 2)
                {
                    startPos = new Vector2Int(x, y);
                    gate.transform.position = ConvertPosMapTo3D(startPos);
                    gate.SetActive(true);
                    continue;
                }

                // End pos
                if (map[y, x] == 3)
                {
                    endPos = new Vector2Int(x, y);
                    king.transform.position = ConvertPosMapTo3D(endPos);
                    king.SetActive(true);
                }
            }
        }
    }

    private void GenerateWaypoints()
    {
        Vector2Int target = startPos;
        Vector2Int direction = Vector2Int.zero;

        while (target != endPos)
        {
            // Get new direction
            direction = GetNewDirection(target, direction);

            // Get new target
            target = GetNextTarget(target, direction);

            // Add target into points
            points.Add(ConvertPosMapTo3D(target));
        }
    }

    private Vector2Int GetNewDirection(Vector2Int start, Vector2Int oldDirection)
    {
        foreach (Vector2Int dir in directions)
        {
            if (dir == oldDirection || dir == -oldDirection)
                continue;

            if (GetPositionValue(start + dir) == 0 || start + dir ==  endPos)
            {
                // if dir is vector up or down
                return (dir.x == 0) ? -dir : dir;
            }
        }

        return Vector2Int.zero;
    }

    private Vector2Int GetNextTarget(Vector2Int oldTarget, Vector2Int direction)
    {
        Vector2Int newTarget = oldTarget;
        if (direction.x == 0)
            direction *= -1;

        while (GetPositionValue(newTarget + direction) == 0 || newTarget + direction == endPos)
        {
            newTarget += direction;
        }

        return newTarget;
    }

    private Vector3 ConvertPosMapTo3D(Vector2Int mapPos)
    {
        Vector2 pos2D = (mapPos - centerPos) * posPrefab.size;
        return new Vector3(pos2D.x, 0, -pos2D.y);
    }

    private int GetPositionValue(Vector2Int pos)
    {
        if (pos.y < 0 || pos.y >= numRows || pos.x < 0 || pos.x >= numCols)
            return -1;

        return map[pos.y, pos.x];
    }

    public void SelectingPosition()
    {
        foreach(Position pos in positions)
        {
            pos.Active();
        }
    }

    public void CompeleteSelect()
    {
        foreach (Position pos in positions)
        {
            pos.Inactive();
        }
    }

    public void Reset()
    {
        foreach (Position pos in positions)
        {
            pos.Reset();
        }
    }
}
