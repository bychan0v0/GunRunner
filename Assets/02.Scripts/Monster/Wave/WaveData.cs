using UnityEngine;

[System.Serializable]
public class WaveData {
    public int waveIndex;       // ���̺� ��ȣ
    public float duration = 60; // ���� �ð�
    public bool isBossWave;     // ���� ���̺� ����
    public int monsterCount;    // ��ȯ ���� ��
    public GameObject[] monsterPrefabs; // ��ȯ�� ���� ����
}
