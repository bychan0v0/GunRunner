using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/Wave/WaveData")]
public class WaveData : ScriptableObject
{
    public int waveIndex;       // ���̺� ��ȣ
    public float duration = 60; // ���� �ð�
    public bool isBossWave;     // ���� ���̺� ����
    public int monsterCount;    // ��ȯ ���� ��
    public GameObject[] monsterPrefabs; // ��ȯ�� ���� ����
}
