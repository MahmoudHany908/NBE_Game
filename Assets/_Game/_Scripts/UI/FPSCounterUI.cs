using UnityEngine;
using TMPro;
using System.Collections;

public class FPSCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro UI element used to display the FPS.")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Settings")]
    [Tooltip("How often (in seconds) the FPS text should update.")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color goodFpsColor = Color.green;
    [SerializeField] private Color okayFpsColor = Color.yellow;
    [SerializeField] private Color badFpsColor = Color.red;

    private int framesAccumulated = 0;
    private float timeAccumulated = 0f;

    private void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("[FPSCounterUI] TextMeshProUGUI component is not assigned!", this);
            enabled = false;
            return;
        }

        StartCoroutine(UpdateFPSCoroutine());
    }

    private void Update()
    {
        // Accumulate time and frame count
        timeAccumulated += Time.unscaledDeltaTime;
        framesAccumulated++;
    }

    private IEnumerator UpdateFPSCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(updateInterval);

            // Calculate FPS
            float currentFps = framesAccumulated / timeAccumulated;
            int roundedFps = Mathf.RoundToInt(currentFps);

            // Update UI Text
            fpsText.text = $"FPS: {roundedFps}";

            // Update UI Color
            if (roundedFps >= 40)
            {
                fpsText.color = goodFpsColor;
            }
            else if (roundedFps >= 20)
            {
                fpsText.color = okayFpsColor;
            }
            else
            {
                fpsText.color = badFpsColor;
            }

            // Reset accumulators
            framesAccumulated = 0;
            timeAccumulated = 0f;
        }
    }
}
