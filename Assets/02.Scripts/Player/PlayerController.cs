using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 720f; // 초당 회전 속도

    [Header("Input")]
    [SerializeField] private Joystick joystick; // FloatingJoystick 연결

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ; // 쓰러지지 않도록 회전 잠금
    }

    private void FixedUpdate()
    {
        if (joystick == null) return;

        // 조이스틱 입력 읽기
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        Vector3 dir = new Vector3(h, 0f, v);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            // 이동
            Vector3 move = dir * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + move);

            // 바라보는 방향 회전
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }
}