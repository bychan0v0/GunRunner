using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;   // 플레이어 Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 20, -10); // 카메라 위치 오프셋 (위에서 내려다보는 느낌)
    
    private void LateUpdate()
    {
        if (player == null) return;

        // 플레이어 기준 위치
        Vector3 targetPos = player.position + offset;

        // x, y는 고정하고 z만 플레이어와 함께 이동
        targetPos.x = transform.position.x;
        targetPos.y = transform.position.y;

        transform.position = targetPos;

        // 항상 위에서 바라보도록 회전 고정
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
