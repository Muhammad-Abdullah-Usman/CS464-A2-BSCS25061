using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1.8f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] float fallMultiplier = 2.2f;

    Rigidbody rb;
    Transform cam;

    Vector3 move;

    bool isGrounded;
    bool jumpQueued;

    float lastGroundedTime = -1f;
    float lastPressedTime = -1f;

    // Used for the visible ground-check gizmo
    Vector3 lastGroundPoint;
    Vector3 lastGroundNormal = Vector3.up;
    bool hasGroundContact;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    void Update()
    {
        // MOVEMENT INPUT
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Camera-relative movement
        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cam != null)
        {
            forward = cam.forward;
            right = cam.right;

            // Flatten camera directions onto the ground
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }

        move = forward * v + right * h;

        // Prevent faster diagonal movement
        move = Vector3.ClampMagnitude(move, 1f);

        // COYOTE TIME
        if (isGrounded &&
            !jumpQueued &&
            rb.linearVelocity.y <= 0f)
        {
            lastGroundedTime = Time.time;
        }

        // JUMP BUFFER
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        if (jumpPressed)
        {
            lastPressedTime = Time.time;
        }

        bool canJump =
            Time.time - lastGroundedTime <= coyoteTime;

        bool wantJump =
            Time.time - lastPressedTime <= jumpBuffer;

        // Queue the jump when both windows are open
        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;

            // Use up both windows
            lastGroundedTime = -1f;
            lastPressedTime = -1f;
        }
    }

    void FixedUpdate()
    {
        // Ground contacts from the next physics step
        // will set this back to true if appropriate.
        isGrounded = false;

        // MOVEMENT
        rb.linearVelocity = new Vector3(
            move.x * speed,
            rb.linearVelocity.y,
            move.z * speed
        );

        // JUMP
        if (jumpQueued)
        {
            // Zero vertical velocity first so a buffered/coyote
            // jump while falling still reaches full height.
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);

            rb.AddForce(
                Vector3.up * rb.mass * v0,
                ForceMode.Impulse
            );

            jumpQueued = false;
        }

        // FASTER FALL
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(
                Physics.gravity * (fallMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Only upward-facing contact counts as ground.
            // Walls therefore do NOT count.
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;

                lastGroundPoint = contact.point;
                lastGroundNormal = contact.normal;
                hasGroundContact = true;

                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (hasGroundContact)
        {
            Gizmos.DrawSphere(lastGroundPoint, 0.06f);
            Gizmos.DrawRay(
                lastGroundPoint,
                lastGroundNormal
            );
        }
    }
}