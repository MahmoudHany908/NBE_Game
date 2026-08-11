using UnityEngine;
using Unity.Cinemachine;

public class CameraTargetZone : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The static Cinemachine Camera to activate while the player is inside this trigger. Just place and aim it however you want in the scene.")]
    public CinemachineCamera zoneCamera;

    [Tooltip("The player transform. Assign this in the Inspector.")]
    public Transform player;

    [Header("Priority")]
    [Tooltip("Priority to give the zone camera while inside the trigger. Should be higher than your normal camera's priority.")]
    public int insidePriority = 20;

    private int normalPriority;

    private void Awake()
    {
        // Make sure this object's collider works as a trigger.
        Collider collider3D = GetComponent<Collider>();
        if (collider3D != null)
        {
            collider3D.isTrigger = true;
        }

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = true;
        }

        if (zoneCamera != null)
        {
            // Remember whatever priority it started with so we can restore it on exit.
            normalPriority = zoneCamera.Priority.Value;
        }
    }

    // 3D trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other.transform)) return;
        EnterZone();
    }

    // 3D trigger exit
    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other.transform)) return;
        ExitZone();
    }

    // 2D trigger enter
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other.transform)) return;
        EnterZone();
    }

    // 2D trigger exit
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other.transform)) return;
        ExitZone();
    }

    private bool IsPlayer(Transform otherTransform)
    {
        if (otherTransform == null) return false;

        if (player != null)
        {
            if (otherTransform == player) return true;
            if (otherTransform.IsChildOf(player)) return true;
        }

        // Optional fallback: works if your player GameObject has the "Player" tag.
        return otherTransform.CompareTag("Player");
    }

    public void EnterZone()
    {
        if (zoneCamera == null) return;

        zoneCamera.Priority.Value = insidePriority;
        zoneCamera.Priority.Enabled = true;
    }

    public void ExitZone()
    {
        if (zoneCamera == null) return;

        zoneCamera.Priority.Value = normalPriority;
    }
}