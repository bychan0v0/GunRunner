using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public event Action OnKilled;

    public void Kill()
    {
        OnKilled?.Invoke();
        // 풀 반납 혹은 Destroy
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        // 핸들러 정리(풀링 시 중요)
        OnKilled = null;
    }
}