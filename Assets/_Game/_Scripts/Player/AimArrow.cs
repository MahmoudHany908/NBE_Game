using UnityEngine;

public class AimArrow : MonoBehaviour
{
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform playerRoot;

    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float yawOffset = 0f;

    [Tooltip("If true, arrow hides when there is no aim direction yet.")]
    [SerializeField] private bool hideWhenNoAim = true;

    private Vector3 smoothedDirection = Vector3.forward;

    private void Awake()
    {
        if (playerInputs == null)
            playerInputs = GetComponentInParent<PlayerInputs>();

        if (playerRoot == null)
            playerRoot = transform.root;
    }

    private void LateUpdate()
    {
        if (playerInputs == null || arrow == null)
            return;

        bool hasAim = playerInputs.HasLastAimDirection;
        Vector3 targetDirection = playerInputs.LastAimDirection;

        if (hideWhenNoAim && arrow.gameObject.activeSelf != hasAim)
            arrow.gameObject.SetActive(hasAim);

        if (!hasAim)
            return;

        smoothedDirection = Vector3.Slerp(
            smoothedDirection,
            targetDirection,
            Time.deltaTime * rotationSpeed
        );

        smoothedDirection.y = 0f;

        if (smoothedDirection.sqrMagnitude < 0.0001f)
            smoothedDirection = Vector3.forward;

        float worldYaw = Mathf.Atan2(smoothedDirection.x, smoothedDirection.z) * Mathf.Rad2Deg;
        worldYaw += yawOffset;

        arrow.rotation = Quaternion.Euler(0f, worldYaw, 0f);
    }
}