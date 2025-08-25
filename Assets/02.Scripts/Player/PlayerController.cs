using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private static readonly int SPEED = Animator.StringToHash("Speed");

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 720f; // 초당 회전 속도
    [SerializeField] float moveThreshold = 0.05f;   // Run 판정
    [SerializeField] float stopThreshold = 0.02f;   // Idle 판정

    [Header("Input")]
    [SerializeField] private Joystick joystick; // FloatingJoystick 연결

    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ; // 쓰러지지 않도록 회전 잠금
    }

    private void FixedUpdate()
    {
        if (joystick == null) return;

        // 조이스틱 입력 읽기
        Vector3 raw = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        float inputMag = raw.magnitude;                  // == Speed 파라미터
        animator.SetFloat(SPEED, inputMag);

        Vector3 dir = raw;

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