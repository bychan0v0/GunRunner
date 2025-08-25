using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;   // �÷��̾� Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 20, -10); // ī�޶� ��ġ ������ (������ �����ٺ��� ����)
    
    private void LateUpdate()
    {
        if (player == null) return;

        // �÷��̾� ���� ��ġ
        Vector3 targetPos = player.position + offset;

        // x, y�� �����ϰ� z�� �÷��̾�� �Բ� �̵�
        targetPos.x = transform.position.x;
        targetPos.y = transform.position.y;

        transform.position = targetPos;

        // �׻� ������ �ٶ󺸵��� ȸ�� ����
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
