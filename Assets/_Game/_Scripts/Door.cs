using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Settings")]
    [SerializeField] private float openSpeed = 1f;
    [SerializeField] private string playerTag = "Player";

    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private int entitiesInTrigger = 0;

    private Vector3 leftInitialScale;
    private Vector3 rightInitialScale;

    private void Awake()
    {
        if (leftDoor) leftInitialScale = leftDoor.localScale;
        if (rightDoor) rightInitialScale = rightDoor.localScale;
    }

    private void Update()
    {
        // Move progress towards the target
        if (Mathf.Abs(currentProgress - targetProgress) > 0.001f)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, openSpeed * Time.deltaTime);
            ApplyScale();
        }
    }

    private void ApplyScale()
    {
        // Scale Z goes from 1 (Closed) to 0.05 (Open) to prevent complete collapse/culling issues
        float scaleZ = Mathf.Lerp(1f, 0.05f, currentProgress);

        if (leftDoor)
        {
            leftDoor.localScale = new Vector3(leftInitialScale.x, leftInitialScale.y, scaleZ * leftInitialScale.z);
        }

        if (rightDoor)
        {
            rightDoor.localScale = new Vector3(rightInitialScale.x, rightInitialScale.y, scaleZ * rightInitialScale.z);
        }
    }

    /// <summary>
    /// Call this to open the door.
    /// </summary>
    public void Open()
    {
        targetProgress = 1f;
    }

    /// <summary>
    /// Call this to close the door.
    /// </summary>
    public void Close()
    {
        targetProgress = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
        {
            return;
        }
        //Debug.Log("Enter");
        entitiesInTrigger++;
        UpdateDoorState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
        {
            return;
        }

        entitiesInTrigger--;
        if (entitiesInTrigger < 0) entitiesInTrigger = 0;
        //Debug.Log("Exit");
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        if (entitiesInTrigger > 0)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
}
