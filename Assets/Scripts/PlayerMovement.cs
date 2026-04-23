using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private InputSystem_Actions PlayerInput;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool jumpPressed;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerInput = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        PlayerInput.Enable();

        PlayerInput.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        PlayerInput.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        PlayerInput.Player.Sprint.performed += _ => isSprinting = true;
        PlayerInput.Player.Sprint.canceled += _ => isSprinting = false;

        PlayerInput.Player.Jump.performed += _ => jumpPressed = true;
    }

    private void OnDisable()
    {
        PlayerInput.Disable();
    }

    private void Update()
    {
        isGrounded = CheckGroundBox();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        ApplyBetterJumpPhysics();
    }

    private bool CheckGroundBox()
    {
        return Physics2D.BoxCast(
            groundCheck.position,
            new Vector2(0.8f, 0.1f),
            0f,
            Vector2.down,
            0.05f,
            groundLayer
        );
    }

    private void HandleMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        if (isSprinting)
            targetSpeed *= sprintMultiplier;

        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float speedDif = targetSpeed - rb.linearVelocity.x;
        float movement = speedDif * accelRate;

        rb.AddForce(Vector2.right * movement);
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        jumpPressed = false;
    }

    private void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Keyboard.current.spaceKey.isPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}