using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 720f; // �ʴ� ȸ�� �ӵ�

    [Header("Input")]
    [SerializeField] private Joystick joystick; // FloatingJoystick ����

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ; // �������� �ʵ��� ȸ�� ���
    }

    private void FixedUpdate()
    {
        if (joystick == null) return;

        // ���̽�ƽ �Է� �б�
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        Vector3 dir = new Vector3(h, 0f, v);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            // �̵�
            Vector3 move = dir * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + move);

            // �ٶ󺸴� ���� ȸ��
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }
}