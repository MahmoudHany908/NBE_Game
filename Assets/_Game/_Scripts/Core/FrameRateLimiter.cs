using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [Header("Frame Rate Settings")]
    [Tooltip("Target frame rate. Set to -1 for uncapped.")]
    [SerializeField] private int targetFrameRate = 60;

    // [Tooltip("Disable VSync. If VSync is enabled, the target frame rate will be ignored.")]
    // [SerializeField] private bool disableVSync = true;

    private void Awake()
    {
        ApplyFrameRateSettings();
    }

    private void ApplyFrameRateSettings()
    {
        // 0 = Don't Sync (Vsync off)
        // 1 = Every V Blank
        // 2 = Every Second V Blank
        // QualitySettings.vSyncCount = disableVSync ? 0 : 1;

        Application.targetFrameRate = targetFrameRate;

        Debug.Log($"[FrameRateLimiter] Target FPS set to {targetFrameRate}.");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // This allows changing the frame rate in the inspector during Play Mode
        if (Application.isPlaying)
        {
            ApplyFrameRateSettings();
        }
    }
#endif
}
