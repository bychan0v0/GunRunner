using System.Collections.Generic;
using UnityEngine;

public static class GridUtils
{
    public static bool IsInsideRect(int x, int y, Vector2Int minInc, Vector2Int maxInc)
        => x >= minInc.x && x <= maxInc.x && y >= minInc.y && y <= maxInc.y;

    public static bool IsNear(Vector2Int c, int x, int y, int r)
    {
        int dx = x - c.x, dy = y - c.y;
        return dx*dx + dy*dy <= r*r;
    }

    public static bool IsNearAny(List<Vector2Int> points, int x, int y, int r)
    {
        for (int i = 0; i < points.Count; i++)
            if (IsNear(points[i], x, y, r)) return true;
        return false;
    }

    public static Vector3 LocalToWorld(Transform parent, float tileSize, int gridX, int gridY)
        => parent.position + new Vector3(gridX * tileSize, 0f, gridY * tileSize);
}