using UnityEngine;

[System.Serializable]
public class WaveData {
    public int waveIndex;       // 웨이브 번호
    public float duration = 60; // 제한 시간
    public bool isBossWave;     // 보스 웨이브 여부
    public int monsterCount;    // 소환 몬스터 수
    public GameObject[] monsterPrefabs; // 소환할 몬스터 종류
}
