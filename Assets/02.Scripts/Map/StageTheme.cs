using UnityEngine;

[CreateAssetMenu(fileName = "StageTheme", menuName = "Scriptable Objects/Map/StageTheme")]
public class StageTheme : ScriptableObject
{
    public GameObject[] floorTiles;   // ¹Ù´Úµé
    public GameObject[] wallTiles;    // º®µé

    public GameObject[] obstacleTiles;
    public GameObject[] decoTiles;

    public GameObject playerSpawnMarkerPrefab;
}
