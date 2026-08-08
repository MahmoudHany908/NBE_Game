using UnityEngine;

public class SpineAimRotator : MonoBehaviour
{
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private Transform playerRoot;

    [Tooltip("Assign spine bones from lower to upper, for example: Spine, Chest, UpperChest.")]
    [SerializeField] private Transform[] spineBones;

    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float maxLocalYaw = 80f;

    private Quaternion[] baseLocalRotations;
    private Vector3 smoothedDirection = Vector3.forward;

    private void Awake()
    {
        if (playerInputs == null)
            playerInputs = GetComponentInParent<PlayerInputs>();

        if (playerRoot == null)
            playerRoot = transform.root;
    }

    private void Start()
    {
        if (spineBones == null || spineBones.Length == 0)
        {
            enabled = false;
            return;
        }

        baseLocalRotations = new Quaternion[spineBones.Length];

        for (int i = 0; i < spineBones.Length; i++)
        {
            if (spineBones[i] != null)
                baseLocalRotations[i] = spineBones[i].localRotation;
        }
    }

    private void LateUpdate()
    {
        if (playerInputs == null || spineBones == null || spineBones.Length == 0)
            return;

        if (playerInputs.HasLastAimDirection)
        {
            smoothedDirection = Vector3.Slerp(
                smoothedDirection,
                playerInputs.LastAimDirection,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            smoothedDirection = Vector3.Slerp(
                smoothedDirection,
                playerRoot.forward,
                Time.deltaTime * rotationSpeed
            );
        }

        smoothedDirection.y = 0f;

        if (smoothedDirection.sqrMagnitude < 0.0001f)
            smoothedDirection = Vector3.forward;

        Vector3 localAim = playerRoot.InverseTransformDirection(smoothedDirection);

        float yaw = Mathf.Atan2(localAim.x, localAim.z) * Mathf.Rad2Deg;
        float clampedYaw = Mathf.Clamp(yaw, -maxLocalYaw, maxLocalYaw);

        float yawPerBone = clampedYaw / spineBones.Length;

        for (int i = 0; i < spineBones.Length; i++)
        {
            if (spineBones[i] == null)
                continue;

            spineBones[i].localRotation =
                baseLocalRotations[i] * Quaternion.Euler(0f, yawPerBone, 0f);
        }
    }
}