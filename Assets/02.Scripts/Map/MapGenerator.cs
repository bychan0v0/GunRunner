using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private StageTheme theme;

    [Min(1)] [SerializeField] private int roomCount = 8;
    
    // === 방 크기와 내부(벽으로 둘러싸일) 공간 크기 ===
    [Header("Sizes")]
    [SerializeField] private Vector2Int roomSize = new Vector2Int(14, 10); // 전체 맵(그리드) 크기
    [SerializeField] private Vector2Int innerSize = new Vector2Int(10, 8); // 벽으로 둘러싸인 내부 공간 크기
    [SerializeField] private float tileSize = 1.0f;                        // 타일 월드 간격
    [Range(1, 5)] [SerializeField] private int wallRingThickness = 1;      // 벽/바깥 고지대 두께(타일 수)

    // === 높이 ===
    [Header("Heights")]
    [SerializeField] private float innerY = 0f;     // 내부 바닥 높이
    [SerializeField] private float outerY = 1f;     // 바깥(벽 포함) 높이 = 내부보다 +1
    
    [SerializeField] private float roomGap = 3.0f;  // 방과 방 사이 간격

    [SerializeField] private int seed = 12345;

    // 배치 규칙
    [Header("Placement")]
    [Range(0f, 0.5f)] [SerializeField] private float obstacleDensity = 0.08f; // 내부용(장애물만)
    [Range(0f, 0.5f)] [SerializeField] private float decoDensity     = 0.10f; // 외부용(장식만)
    [SerializeField] private float obstacleYOffset = 1f; // 내부 바닥보다 +1
    [SerializeField] private int innerSafeRadius   = 2;  // 스폰 주변 안전 구역(원하면 0)

    [SerializeField] private Transform stageRoot;

    System.Random rng;

    // ======= 에디터에서 버튼처럼 쓰기 =======
    [ContextMenu("Generate Stage")]
    public void GenerateStage()
    {
        if (theme == null)
        {
            Debug.LogError("[MapGenerator] StageTheme 이 비어있어요.");
            return;
        }

        if (stageRoot == null)
        {
            var root = GameObject.Find("StageRoot");
            stageRoot = root ? root.transform : new GameObject("StageRoot").transform;
            stageRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        ClearStage();

        rng = new System.Random(seed);
        Vector3 cursor = Vector3.zero;

        for (int i = 0; i < roomCount; i++)
        {
            string roomName = $"Room_{i:00}";
            var roomGo = new GameObject(roomName);
            roomGo.transform.SetParent(stageRoot, false);
            roomGo.transform.position = cursor;

            GenerateSingleRoom(roomGo.transform, i);

            // 다음 방 위치로 커서 이동 (가로로 나열)
            cursor += new Vector3((roomSize.x + roomGap) * tileSize, 0f, 0f);
        }
    }

    [ContextMenu("Regenerate (Same Seed)")]
    public void RegenerateSameSeed()
    {
        GenerateStage();
    }

    [ContextMenu("Regenerate (New Seed)")]
    public void RegenerateNewSeed()
    {
        seed = UnityEngine.Random.Range(int.MinValue / 2, int.MaxValue / 2);
        GenerateStage();
    }

    [ContextMenu("Clear Stage")]
    public void ClearStage()
    {
        if (stageRoot == null) return;
        for (int i = stageRoot.childCount - 1; i >= 0; i--)
        {
            var c = stageRoot.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(c.gameObject);
            else Destroy(c.gameObject);
#else
            Destroy(c.gameObject);
#endif
        }
    }

    // ======= 단일 방 생성 =======
    void GenerateSingleRoom(Transform roomParent, int roomIndex)
    {
        // 0) 파라미터 정리/보정
        innerSize.x = Mathf.Clamp(innerSize.x, 1, roomSize.x - 2 * wallRingThickness);
        innerSize.y = Mathf.Clamp(innerSize.y, 1, roomSize.y - 2 * wallRingThickness);

        // 내부 공간을 전체 맵의 중앙에 정렬
        Vector2Int innerStart = new Vector2Int(
            Mathf.Clamp((roomSize.x - innerSize.x) / 2, wallRingThickness, roomSize.x - innerSize.x - wallRingThickness),
            Mathf.Clamp((roomSize.y - innerSize.y) / 2, wallRingThickness, roomSize.y - innerSize.y - wallRingThickness)
        );
        Vector2Int innerEnd = new Vector2Int(innerStart.x + innerSize.x - 1, innerStart.y + innerSize.y - 1);

        // 링(벽이 깔리는 띠)을 포함하는 사각형 경계
        int ringMinX = Mathf.Max(0, innerStart.x - wallRingThickness);
        int ringMaxX = Mathf.Min(roomSize.x - 1, innerEnd.x   + wallRingThickness);
        int ringMinY = Mathf.Max(0, innerStart.y - wallRingThickness);
        int ringMaxY = Mathf.Min(roomSize.y - 1, innerEnd.y   + wallRingThickness);

        // === 스폰 타일: inner 중앙 1칸 ===
        Vector2Int centerTile = new Vector2Int(
            Mathf.RoundToInt((innerStart.x + innerEnd.x) * 0.5f),
            Mathf.RoundToInt((innerStart.y + innerEnd.y) * 0.5f)
        );
        var spawnTiles = new List<Vector2Int> { centerTile };

        // 1) 바닥: 내부는 floorTiles(랜덤)@innerY, 외부/링은 "벽 타일(첫 번째)@outerY"로 통일
        GameObject outerTilePrefab = GetFirst(theme.wallTiles);
        if (outerTilePrefab == null)
        {
            Debug.LogWarning("[MapGenerator] wallTiles가 비어있어 외부 바닥을 생성할 수 없습니다.");
        }

        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                bool isInside =
                    (x >= innerStart.x && x <= innerEnd.x &&
                     y >= innerStart.y && y <= innerEnd.y);

                if (isInside)
                {
                    var floor = Pick(theme.floorTiles);
                    if (floor != null)
                        InstantiateAtHeight(floor, roomParent, x, y, innerY);
                }
                else
                {
                    if (outerTilePrefab != null)
                        InstantiateAtHeight(outerTilePrefab, roomParent, x, y, outerY);
                }
            }
        }

        // 2) 벽: 내부 경계 바깥쪽 링에 배치 (두께 지원) ? 항상 outerY(=innerY+1)
        for (int t = 0; t < wallRingThickness; t++)
        {
            int minX = innerStart.x - 1 - t;
            int maxX = innerEnd.x   + 1 + t;
            int minY = innerStart.y - 1 - t;
            int maxY = innerEnd.y   + 1 + t;

            // 상하 라인
            for (int x = Mathf.Max(0, minX); x <= Mathf.Min(roomSize.x - 1, maxX); x++)
            {
                TryPlaceWall(roomParent, x, minY);
                TryPlaceWall(roomParent, x, maxY);
            }
            // 좌우 라인
            for (int y = Mathf.Max(0, minY + 1); y <= Mathf.Min(roomSize.y - 1, maxY - 1); y++)
            {
                TryPlaceWall(roomParent, minX, y);
                TryPlaceWall(roomParent, maxX, y);
            }
        }

        // 3) 스폰 배치 (첫 방에만, 내부 높이)
        if (roomIndex == 0 && theme.playerSpawnMarkerPrefab != null)
        {
            foreach (var tile in spawnTiles)
            {
                var spawn = InstantiateAtHeight(theme.playerSpawnMarkerPrefab, roomParent, tile.x, tile.y, innerY);
                spawn.name = "PlayerSpawn";
            }
        }

        // 4) 배치 규칙
        // A) 내부: 장식 ?, "장애물만" 배치, 높이 = innerY + obstacleYOffset
        if (theme.obstacleTiles != null && theme.obstacleTiles.Length > 0 && obstacleDensity > 0f)
        {
            for (int x = innerStart.x; x <= innerEnd.x; x++)
            for (int y = innerStart.y; y <= innerEnd.y; y++)
            {
                // 스폰 주변 안전 구역 비우기
                if (IsNearAny(spawnTiles, x, y, innerSafeRadius)) continue;

                if (rng.NextDouble() < obstacleDensity)
                {
                    var obs = Pick(theme.obstacleTiles);
                    if (obs != null) InstantiateAtHeight(obs, roomParent, x, y, innerY + obstacleYOffset);
                }
            }
        }

        // B) 외부: 장애물 ?, "장식만" 배치, 높이 = outerY
        if (theme.decoTiles != null && theme.decoTiles.Length > 0 && decoDensity > 0f)
        {
            for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
            {
                bool insideInner = IsInsideRect(x, y, innerStart, innerEnd);
                bool insideRing  = IsInsideRect(x, y, new Vector2Int(ringMinX, ringMinY),
                                                     new Vector2Int(ringMaxX, ringMaxY));

                // 외부 = 전체 - (내부 ∪ 링 사각형)
                bool isOuterFringe = !insideInner && !insideRing;
                if (!isOuterFringe) continue;

                if (rng.NextDouble() < decoDensity)
                {
                    var deco = Pick(theme.decoTiles);
                    if (deco != null) InstantiateAtHeight(deco, roomParent, x, y, outerY);
                }
            }
        }
    }

    // ======= 유틸 =======
    GameObject InstantiateAtHeight(GameObject prefab, Transform parent, int gridX, int gridY, float y)
    {
        Vector3 pos = LocalToWorld(parent, gridX, gridY);
        pos.y = y;
        return Instantiate(prefab, pos, Quaternion.identity, parent);
    }

    void TryPlaceWall(Transform parent, int x, int y)
    {
        if (x < 0 || y < 0 || x >= roomSize.x || y >= roomSize.y) return;
        var wall = Pick(theme.wallTiles);
        if (wall == null) return;
        InstantiateAtHeight(wall, parent, x, y, outerY); // 벽은 바깥 높이(=Y+1)에
    }
    
    GameObject Pick(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        int idx = rng.Next(arr.Length);
        return arr[idx];
    }

    GameObject GetFirst(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[0];
    }

    GameObject InstantiateAt(GameObject prefab, Transform parent, int gridX, int gridY)
    {
        Vector3 pos = LocalToWorld(parent, gridX, gridY);
        var go = Instantiate(prefab, pos, Quaternion.identity, parent);
        return go;
    }

    Vector3 LocalToWorld(Transform parent, int gridX, int gridY)
    {
        return parent.position + new Vector3(gridX * tileSize, 0f, gridY * tileSize);
    }

    bool IsInsideRect(int x, int y, Vector2Int minInclusive, Vector2Int maxInclusive)
    {
        return x >= minInclusive.x && x <= maxInclusive.x &&
               y >= minInclusive.y && y <= maxInclusive.y;
    }

    bool IsNear(Vector2Int c, int x, int y, int r)
    {
        int dx = x - c.x; int dy = y - c.y;
        return dx*dx + dy*dy <= r*r;
    }

    bool IsNearAny(List<Vector2Int> points, int x, int y, int r)
    {
        foreach (var p in points)
            if (IsNear(p, x, y, r)) return true;
        return false;
    }
}
