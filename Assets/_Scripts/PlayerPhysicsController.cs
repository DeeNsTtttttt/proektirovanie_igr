using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerPhysicsController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpImpulse = 7f;
    [SerializeField] private float rotationSpeed = 12f;

    private Rigidbody rb;
    private Collider col;
    private Vector3 moveInput;
    private bool jumpRequested;
    private bool runHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveInput = ReadMoveInput();
        runHeld = ReadRunHeld();
        jumpRequested |= ReadJumpPressed();
    }

    private void FixedUpdate()
    {
        Move();

        if (jumpRequested)
        {
            jumpRequested = false;
            if (IsGrounded())
            {
                Vector3 v = rb.linearVelocity;
                v.y = 0f;
                rb.linearVelocity = v;
                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            }
        }
    }

    private void Move()
    {
        float currentSpeed = runHeld ? runSpeed : moveSpeed;
        Vector3 delta = moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + delta);

        if (moveInput.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(moveInput, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private bool IsGrounded()
    {
        float rayDistance = col.bounds.extents.y + 0.25f;
        return Physics.Raycast(col.bounds.center, Vector3.down, rayDistance);
    }

    private Vector3 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return Vector3.zero;

        float x = 0f;
        float z = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) z -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) z += 1f;

        return new Vector3(x, 0f, z).normalized;
#else
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
#endif
    }

    private bool ReadJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump");
#endif
    }

    private bool ReadRunHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }
}
