using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static PlayerReference Instance { get; private set; }
    public Transform PlayerTransform { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        PlayerTransform = transform;
    }
}