using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("MovementVariables")]
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float sprintMuiltiplair;
    [SerializeField] private float downwordForce;
    [SerializeField] private float maxSlopeAngle = 45f;



    [Header("GroundCheackSettings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayLength;
    [SerializeField] private float rayOffset;



    [Header("LocalVariables")]
    private Player player;
    private Vector3 moveDirection;
    private Collider col;


    public Vector3 MovementDirection => moveDirection;


    void Start()
    {
        player = GetComponent<Player>();
        col = GetComponent<Collider>();
    }


    void FixedUpdate()
    {

        HandleSlopes();

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            ApplyMovement(MoveSpeed * sprintMuiltiplair);
        }
        else
        {
            ApplyMovement(MoveSpeed);

        }

    }
    private void ApplyMovement(float speed)
    {
        if (IsGrounded())
        {

            Vector3 targetVelocity = moveDirection * speed;
            targetVelocity.y = player.GetPlayerRigidBody().linearVelocity.y;
            player.GetPlayerRigidBody().linearVelocity = targetVelocity;

        }
        if (!IsGrounded())
        {
            player.GetPlayerRigidBody().AddForce(Vector3.down * downwordForce, ForceMode.VelocityChange);
        }
    }


    private void HandleSlopes()
    {
        Vector3 rawInput = player.GetPlayerInputs().MoveDirection();

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayLength + 0.3f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle <= maxSlopeAngle && slopeAngle > 0.1f)
            {
                moveDirection = Vector3.ProjectOnPlane(rawInput, hit.normal).normalized;
                return;
            }
        }

        // Fallback: flat ground or no hit
        moveDirection = rawInput;
    }


    public bool IsGrounded()
    {
        Vector3 bounds = col.bounds.extents;
        Vector3 center = col.bounds.center;

        // scale bounds by rayOffset to control spread of rays
        float offsetX = bounds.x * rayOffset;
        float offsetZ = bounds.z * rayOffset;

        Vector3[] rayOrigins = new Vector3[4]
        {
            new Vector3(center.x + offsetX, center.y, center.z + offsetZ), // front-right
            new Vector3(center.x - offsetX, center.y, center.z + offsetZ), // front-left
            new Vector3(center.x + offsetX, center.y, center.z - offsetZ), // back-right
            new Vector3(center.x - offsetX, center.y, center.z - offsetZ)  // back-left
        };

        foreach (var origin in rayOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, rayLength, groundMask))
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<CapsuleCollider>();

        Vector3 bounds = col.bounds.extents;
        Vector3 center = col.bounds.center;

        float offsetX = bounds.x * rayOffset;
        float offsetZ = bounds.z * rayOffset;

        Vector3[] rayOrigins = new Vector3[4]
        {
            new Vector3(center.x + offsetX, center.y, center.z + offsetZ),
            new Vector3(center.x - offsetX, center.y, center.z + offsetZ),
            new Vector3(center.x + offsetX, center.y, center.z - offsetZ),
            new Vector3(center.x - offsetX, center.y, center.z - offsetZ)
        };

        Gizmos.color = Color.red;
        foreach (var origin in rayOrigins)
        {
            Gizmos.DrawLine(origin, origin + Vector3.down * rayLength);
        }
    }



}
