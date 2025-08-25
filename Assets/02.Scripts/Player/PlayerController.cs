using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private static readonly int SPEED = Animator.StringToHash("Speed");

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 720f; // �ʴ� ȸ�� �ӵ�
    [SerializeField] float moveThreshold = 0.05f;   // Run ����
    [SerializeField] float stopThreshold = 0.02f;   // Idle ����

    [Header("Input")]
    [SerializeField] private Joystick joystick; // FloatingJoystick ����

    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ; // �������� �ʵ��� ȸ�� ���
    }

    private void FixedUpdate()
    {
        if (joystick == null) return;

        // ���̽�ƽ �Է� �б�
        Vector3 raw = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        float inputMag = raw.magnitude;                  // == Speed �Ķ����
        animator.SetFloat(SPEED, inputMag);

        Vector3 dir = raw;

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