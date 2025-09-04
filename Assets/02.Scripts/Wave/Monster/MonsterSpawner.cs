using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // ���� ��ȯ ��ġ �迭
    [SerializeField] private float spawnInterval = 0.5f; // ���� ��ȯ ����

    // ���̺� ���� �� ȣ�� (WaveManager���� ȣ��)
    public void BeginSpawn(WaveData wave, IWaveCounters counters, CancellationToken ct)
        => SpawnLoopAsync(wave, counters, ct).Forget();

    // ���� ���� ��ȯ ����
    private async UniTaskVoid SpawnLoopAsync(WaveData wave, IWaveCounters counters, CancellationToken ct)
    {
        int toSpawn = wave.monsterCount;

        for (int i = 0; i < toSpawn; i++)
        {
            ct.ThrowIfCancellationRequested();

            // ���� ��ȯ ��ġ/������ ����
            var point  = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var prefab = wave.monsterPrefabs[UnityEngine.Random.Range(0, wave.monsterPrefabs.Length)];

            // ���� ���� (Ǯ�� ����)
            var go = Spawn(prefab, point.position);

            // ���Ͱ� ���� �� ī���Ϳ� �˸����� �̺�Ʈ ���
            var monster = go.GetComponent<Monster>();
            if (monster != null)
            {
                // Ǯ���� ���� ��ȯ �� �ڵ鷯 ������ ���� �ʿ��� ����� �޸� �� ����
                monster.OnKilled += () => counters.IncKilled();
            }

            // ���� ī��Ʈ ����
            counters.IncSpawned();

            // ���� ���� ��ȯ���� ���
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: ct);
        }
    }

    // Instantiate ��� Ǯ�� �ý��� ���� ����
    private GameObject Spawn(GameObject prefab, Vector3 pos)
    {
        return Instantiate(prefab, pos, Quaternion.identity);
    }
}