using System;
using UnityEngine;

public interface IWaveCounters
{
    int ToSpawn { get; }
    int Spawned { get; }
    int Killed  { get; }

    void IncSpawned();
    void IncKilled();

    event Action OnKilledChanged;
}
