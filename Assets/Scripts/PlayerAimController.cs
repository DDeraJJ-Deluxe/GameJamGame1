using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimPivot;

    [Header("Settings")]
    [SerializeField] private float aimSensitivity = 180f;

    private InputSystem_Actions input;

    private float aimAngle;
    private Vector2 aimAnchor;

    [SerializeField] private float anchorFollowSpeed = 8f;

    private Vector2 smoothedMouse;
    [SerializeField] private float mouseDragSpeed = 20f;

    private bool isAiming;

    private CameraFollow camFollow;


    private void Awake()
    {
        input = new InputSystem_Actions();
        camFollow = FindAnyObjectByType<CameraFollow>();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.AimHold.performed += OnAimStarted;
        input.Player.AimHold.canceled += OnAimStopped;
    }

    private void OnDisable()
    {
        input.Player.AimHold.performed -= OnAimStarted;
        input.Player.AimHold.canceled -= OnAimStopped;

        input.Disable();
    }

    private void OnAimStarted(InputAction.CallbackContext ctx)
    {
        isAiming = true;

        Vector2 mouse = Mouse.current.position.ReadValue();
        smoothedMouse = mouse;
        aimAnchor = mouse;
        
        camFollow.SetAiming(true);
    }

    private void OnAimStopped(InputAction.CallbackContext ctx)
    {
        isAiming = false;

        camFollow.SetAiming(false);
    }

    private void Update()
    {
        if (!isAiming) return;

        SmoothMouse();
        SmoothAnchor();
        HandleAim();
    }

    private void SmoothMouse()
    {
        Vector2 mouse = Mouse.current.position.ReadValue();

        smoothedMouse = Vector2.Lerp(
            smoothedMouse,
            mouse,
            mouseDragSpeed * Time.deltaTime
        );
    }

    private void SmoothAnchor()
    {
        Vector2 delta = smoothedMouse - aimAnchor;

        aimAnchor += delta * anchorFollowSpeed * Time.deltaTime;

        // optional safety clamp (prevents microscopic convergence)
        if (delta.magnitude < 0.01f)
            aimAnchor -= delta * 0.5f;
    }

    private void HandleAim()
    {
        Vector2 delta = smoothedMouse - aimAnchor;

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 rawMouseWorld = cam.ScreenToWorldPoint(
            new Vector3(Mouse.current.position.ReadValue().x,
                        Mouse.current.position.ReadValue().y,
                        -cam.transform.position.z)
        );

        Vector3 smoothMouseWorld = cam.ScreenToWorldPoint(
            new Vector3(smoothedMouse.x,
                        smoothedMouse.y,
                        -cam.transform.position.z)
        );

        Vector3 anchorWorld = cam.ScreenToWorldPoint(
            new Vector3(aimAnchor.x,
                        aimAnchor.y,
                        -cam.transform.position.z)
        );

        Vector3 pivotWorld = aimPivot.position;

        // 🟣 SMOOTH mouse (dragged input)
        Gizmos.color = new Color(0.6f, 0f, 1f); // purple
        Gizmos.DrawSphere(smoothMouseWorld, 0.08f);

        // 🟡 ANCHOR (lag point)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(anchorWorld, 0.1f);

        // 🟠 SMOOTH → ANCHOR line
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(smoothMouseWorld, anchorWorld);
    }
}