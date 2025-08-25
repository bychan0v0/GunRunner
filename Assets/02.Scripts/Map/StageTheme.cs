using UnityEngine;

[CreateAssetMenu(fileName = "StageTheme", menuName = "Scriptable Objects/Map/StageTheme")]
public class StageTheme : ScriptableObject
{
    public GameObject[] floorTiles;   // �ٴڵ�
    public GameObject[] wallTiles;    // ����

    public GameObject[] obstacleTiles;
    public GameObject[] decoTiles;

    public GameObject playerSpawnMarkerPrefab;
    public GameObject portalPrefab;   // ���� ������ �Ѿ�� ����(Archero ��Ÿ��)
}
