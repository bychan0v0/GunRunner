using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // 몬스터 소환 위치 배열
    [SerializeField] private float spawnInterval = 0.5f; // 몬스터 소환 간격

    // 웨이브 시작 시 호출 (WaveManager에서 호출)
    public void BeginSpawn(WaveData wave, IWaveCounters counters, CancellationToken ct)
        => SpawnLoopAsync(wave, counters, ct).Forget();

    // 실제 몬스터 소환 루프
    private async UniTaskVoid SpawnLoopAsync(WaveData wave, IWaveCounters counters, CancellationToken ct)
    {
        int toSpawn = wave.monsterCount;

        for (int i = 0; i < toSpawn; i++)
        {
            ct.ThrowIfCancellationRequested();

            // 랜덤 소환 위치/프리팹 선택
            var point  = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var prefab = wave.monsterPrefabs[UnityEngine.Random.Range(0, wave.monsterPrefabs.Length)];

            // 몬스터 생성 (풀링 권장)
            var go = Spawn(prefab, point.position);

            // 몬스터가 죽을 때 카운터에 알리도록 이벤트 등록
            var monster = go.GetComponent<Monster>();
            if (monster != null)
            {
                // 풀링을 쓰면 반환 시 핸들러 정리를 몬스터 쪽에서 해줘야 메모리 릭 방지
                monster.OnKilled += () => counters.IncKilled();
            }

            // 스폰 카운트 증가
            counters.IncSpawned();

            // 다음 몬스터 소환까지 대기
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: ct);
        }
    }

    // Instantiate 대신 풀링 시스템 연결 가능
    private GameObject Spawn(GameObject prefab, Vector3 pos)
    {
        return Instantiate(prefab, pos, Quaternion.identity);
    }
}