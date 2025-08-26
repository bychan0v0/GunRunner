using System;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public event Action OnKilled;

    public void Kill()
    {
        OnKilled?.Invoke();
        // Ǯ �ݳ� Ȥ�� Destroy
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        // �ڵ鷯 ����(Ǯ�� �� �߿�)
        OnKilled = null;
    }
}