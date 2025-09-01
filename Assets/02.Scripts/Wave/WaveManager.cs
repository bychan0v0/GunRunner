using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveData[] waves;     // ���̺� ���� �迭 (�ν����Ϳ��� ����)
    [SerializeField] private MonsterSpawner spawner;      // ���� ������ ����
    [SerializeField] private float interWaveDelay = 3f; // ���̺� ���� ��� �ð�

    // �ܺ�(UI ��)���� ����� �̺�Ʈ
    public event Action<int> OnWaveStarted;          // ���̺� ���� ��
    public event Action<int> OnWaveCleared;          // ���̺� Ŭ���� ��
    public event Action<int,float> OnWaveTimeTick;   // ���̺� ���� �� Ÿ�̸�(���̺� ��ȣ, ���� ��)

    private CancellationToken _destroyToken; // WaveManager�� �ı��� �� ��ҿ� ��ū

    private void Awake() 
        => _destroyToken = this.GetCancellationTokenOnDestroy();

    private async void Start() // ��Ʈ�� ����Ʈ (���� ���� �� ���̺� ����)
    {
        try 
        { 
            await RunAllWavesAsync(_destroyToken); 
        }
        catch (OperationCanceledException) 
        { 
            // �� ��ȯ ������ WaveManager�� �ı��Ǹ� ����� ��
        }
    }

    // ��ü ���̺긦 ���������� ����
    private async UniTask RunAllWavesAsync(CancellationToken ct)
    {
        for (int i = 0; i < waves.Length; i++)
        {
            var wave = waves[i];

            // ���̺� ���� �̺�Ʈ �߻�
            OnWaveStarted?.Invoke(i + 1);

            // �� ���̺� ����
            await RunOneWaveAsync(wave, i + 1, ct);

            // ���̺� Ŭ���� �̺�Ʈ �߻�
            OnWaveCleared?.Invoke(i + 1);

            // ���̺� ���� �޽� �ð� (��: 3��)
            await UniTask.Delay(TimeSpan.FromSeconds(interWaveDelay), cancellationToken: ct);
        }

        // ��� ���̺� Ŭ���� �� ���� �¸� ó��
        Debug.Log("All waves cleared!");
    }

    // ���� ���̺� ���� (���� ��ȯ + Ÿ�̸� ����)
    private async UniTask RunOneWaveAsync(WaveData wave, int waveIndex, CancellationToken ct)
    {
        // ���̺길�� ��� ��ū (���� ���̺�� �Ѿ �� ������)
        using var waveCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // ���� ī���� (���� ����/ų �� ����)
        var counters = new WaveCounters(wave.monsterCount);

        // ���� ���� ����
        spawner.BeginSpawn(wave, counters, waveCts.Token); // �� IWaveCounters�� ����

        // ���� ���� (��� ���� ���)�� ������ Task
        var clearedTcs = new UniTaskCompletionSource();
        counters.OnKilledChanged += () =>
        {
            if (counters.Killed >= counters.ToSpawn) 
                clearedTcs.TrySetResult();
        };

        // ���� �ð� Ÿ�̸� Task
        var timeTask  = RunWaveTimerAsync(waveIndex, wave.duration, waveCts.Token);
        // ���� ���� Task
        var clearTask = clearedTcs.Task;

        // ���ѽð� or ���� �� ���� ������ ������ ���̺� ����
        await UniTask.WhenAny(timeTask, clearTask);

        // ���̺� ����
        waveCts.Cancel();
    }

    // ���� �ð� Ÿ�̸� (UI�� ���� �ð� ����)
    private async UniTask RunWaveTimerAsync(int waveIndex, float durationSec, CancellationToken ct)
    {
        float remain = durationSec;

        // 0.2�� ������ UI�� ���� �̺�Ʈ ����
        while (remain > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);
            remain -= 0.2f;
            OnWaveTimeTick?.Invoke(waveIndex, Mathf.Max(0, remain));
        }
    }

    // ���̺� ī���� (����/ų ���� ����)
    public class WaveCounters : IWaveCounters
    {
        public int ToSpawn { get; private set; }
        public int Spawned { get; private set; }
        public int Killed  { get; private set; }

        public event Action OnKilledChanged;

        public WaveCounters(int toSpawn) { ToSpawn = toSpawn; }

        public void IncSpawned() => Spawned++;

        public void IncKilled()
        {
            Killed++;
            OnKilledChanged?.Invoke();
        }
    }
}

