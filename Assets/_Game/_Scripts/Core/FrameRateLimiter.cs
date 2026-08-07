using UnityEngine;
using UnityEngine.UI;

public class FrameRateTester : MonoBehaviour
{
    [Header("UI Display (Optional)")]
    [Tooltip("Assign a UI Text element here to see the real-time FPS on your phone.")]
    [SerializeField] private Text fpsCounterText;

    // Internal variables for the FPS counter
    private float deltaTime = 0.0f;
    // private int currentFps = 0.0f;

    private void Start()
    {
        // Default to standard 60 FPS on launch
        SetTo60FPS();
    }

    private void Update()
    {
        // Simple, efficient calculation for the real-time FPS counter
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        // currentFps = 1 / deltaTime;

        if (fpsCounterText != null)
        {
            fpsCounterText.text = $"FPS: {Mathf.Ceil(1/deltaTime)} | Target: {Application.targetFrameRate}";
        }
    }

    // --- BUTTON CLICK FUNCTIONS ---

    /// <summary> Button 1: Cinematic / Battery Saver Mode </summary>
    public void SetTo30FPS()
    {
        ApplyFrameRate(30, "30 FPS (Battery Saver)");
    }

    /// <summary> Button 2: Standard Target Mode </summary>
    public void SetTo60FPS()
    {
        ApplyFrameRate(60, "60 FPS (Standard/Ideal)");
    }

    /// <summary> Button 3: Match Hardware Max Mode </summary>
    public void SetToDeviceMax()
    {
        float hardwareRate = GetDeviceRefreshRate();
        int target = hardwareRate > 0 ? Mathf.RoundToInt(hardwareRate) : 60;

        ApplyFrameRate(target, $"Device Max Hardware ({target}Hz)");
    }

    /// <summary> Button 4 (Bonus!): Uncapped / Stress Test Mode </summary>
    public void SetToUncapped()
    {
        // Note: Mobile hardware VSync will still try to cap this at your max refresh rate, 
        // but this allows you to see if your game can handle raw uncapped performance.
        ApplyFrameRate(-1, "Uncapped Performance Mode");
    }

    // --- HELPER METHODS ---

    private void ApplyFrameRate(int target, string modeName)
    {
        Application.targetFrameRate = target;
        //Debug.Log($"[FrameRateTester] Switched to {modeName}. Target set to {target}.");
    }

    private float GetDeviceRefreshRate()
    {
#if UNITY_2022_2_OR_NEWER
        return (float)Screen.currentResolution.refreshRateRatio.value;
#else
        return Screen.currentResolution.refreshRate;
#endif
    }
}
