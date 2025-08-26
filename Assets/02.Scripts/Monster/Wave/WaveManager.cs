using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private WaveData[] waves;     // 웨이브 정보 배열 (인스펙터에서 설정)
    [SerializeField] private MonsterSpawner spawner;      // 몬스터 스포너 참조
    [SerializeField] private float interWaveDelay = 3f; // 웨이브 사이 대기 시간

    // 외부(UI 등)에서 사용할 이벤트
    public event Action<int> OnWaveStarted;          // 웨이브 시작 시
    public event Action<int> OnWaveCleared;          // 웨이브 클리어 시
    public event Action<int,float> OnWaveTimeTick;   // 웨이브 진행 중 타이머(웨이브 번호, 남은 초)

    private CancellationToken _destroyToken; // WaveManager가 파괴될 때 취소용 토큰

    private void Awake() 
        => _destroyToken = this.GetCancellationTokenOnDestroy();

    private async void Start() // 엔트리 포인트 (게임 시작 시 웨이브 진행)
    {
        try 
        { 
            await RunAllWavesAsync(_destroyToken); 
        }
        catch (OperationCanceledException) 
        { 
            // 씬 전환 등으로 WaveManager가 파괴되면 여기로 옴
        }
    }

    // 전체 웨이브를 순차적으로 실행
    private async UniTask RunAllWavesAsync(CancellationToken ct)
    {
        for (int i = 0; i < waves.Length; i++)
        {
            var wave = waves[i];

            // 웨이브 시작 이벤트 발생
            OnWaveStarted?.Invoke(i + 1);

            // 한 웨이브 실행
            await RunOneWaveAsync(wave, i + 1, ct);

            // 웨이브 클리어 이벤트 발생
            OnWaveCleared?.Invoke(i + 1);

            // 웨이브 사이 휴식 시간 (예: 3초)
            await UniTask.Delay(TimeSpan.FromSeconds(interWaveDelay), cancellationToken: ct);
        }

        // 모든 웨이브 클리어 → 게임 승리 처리
        Debug.Log("All waves cleared!");
    }

    // 개별 웨이브 실행 (몬스터 소환 + 타이머 관리)
    private async UniTask RunOneWaveAsync(WaveData wave, int waveIndex, CancellationToken ct)
    {
        // 웨이브만의 취소 토큰 (다음 웨이브로 넘어갈 때 정리용)
        using var waveCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // 몬스터 카운터 (스폰 개수/킬 수 추적)
        var counters = new WaveCounters(wave.monsterCount);

        // 몬스터 스폰 시작
        spawner.BeginSpawn(wave, counters, waveCts.Token); // ← IWaveCounters로 전달

        // 전멸 조건 (모든 몬스터 사망)을 감시할 Task
        var clearedTcs = new UniTaskCompletionSource();
        counters.OnKilledChanged += () =>
        {
            if (counters.Killed >= counters.ToSpawn) 
                clearedTcs.TrySetResult();
        };

        // 제한 시간 타이머 Task
        var timeTask  = RunWaveTimerAsync(waveIndex, wave.duration, waveCts.Token);
        // 몬스터 전멸 Task
        var clearTask = clearedTcs.Task;

        // 제한시간 or 전멸 중 먼저 끝나는 쪽으로 웨이브 종료
        await UniTask.WhenAny(timeTask, clearTask);

        // 웨이브 정리
        waveCts.Cancel();
    }

    // 제한 시간 타이머 (UI에 남은 시간 전달)
    private async UniTask RunWaveTimerAsync(int waveIndex, float durationSec, CancellationToken ct)
    {
        float remain = durationSec;

        // 0.2초 단위로 UI에 갱신 이벤트 전달
        while (remain > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);
            remain -= 0.2f;
            OnWaveTimeTick?.Invoke(waveIndex, Mathf.Max(0, remain));
        }
    }

    // 웨이브 카운터 (스폰/킬 개수 관리)
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

