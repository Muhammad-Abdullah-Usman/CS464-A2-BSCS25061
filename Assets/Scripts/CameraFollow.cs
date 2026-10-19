using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;

    [Header("Follow")]
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -7f);
    [SerializeField] float sharpness = 5f;

    [Header("Look Ahead")]
    [SerializeField] float lookAhead = 0.35f;

    Rigidbody targetRb;

    void Awake()
    {
        if (target == null)
        {
            PlayerController player =
                FindAnyObjectByType<PlayerController>();

            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();

            // Start at the correct position immediately
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 flatVelocity = Vector3.zero;

        if (targetRb != null)
        {
            flatVelocity = targetRb.linearVelocity;
            flatVelocity.y = 0f;
        }

        // Look slightly ahead in the direction the player is moving
        Vector3 aimPoint =
            target.position + flatVelocity * lookAhead;

        Vector3 wanted =
            aimPoint + offset;

        // Lecture 05 frame rate independent smoothing
        float blend =
            1f - Mathf.Exp(-sharpness * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            wanted,
            blend
        );

        transform.LookAt(aimPoint);
    }
}