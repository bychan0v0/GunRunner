using UnityEngine;

[CreateAssetMenu(fileName = "StageTheme", menuName = "Scriptable Objects/Map/StageTheme")]
public class StageTheme : ScriptableObject
{
    public GameObject[] floorTiles;   // 바닥들
    public GameObject[] wallTiles;    // 벽들

    public GameObject[] obstacleTiles;
    public GameObject[] decoTiles;

    public GameObject playerSpawnMarkerPrefab;
    public GameObject portalPrefab;   // 다음 방으로 넘어가는 포털(Archero 스타일)
}
