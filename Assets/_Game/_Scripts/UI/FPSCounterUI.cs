using UnityEngine;
using TMPro;

public class FPSCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color goodFpsColor = Color.green;
    [SerializeField] private Color okayFpsColor = Color.yellow;
    [SerializeField] private Color badFpsColor = Color.red;

    private int framesAccumulated;
    private float timeAccumulated;

    private void Update()
    {
        timeAccumulated += Time.unscaledDeltaTime;
        framesAccumulated++;

        if (timeAccumulated >= updateInterval)
        {
            float fps = framesAccumulated / timeAccumulated;

            int roundedFps = Mathf.RoundToInt(fps);
            fpsText.text = $"FPS: {roundedFps}";

            if (roundedFps >= 40)
                fpsText.color = goodFpsColor;
            else if (roundedFps >= 20)
                fpsText.color = okayFpsColor;
            else
                fpsText.color = badFpsColor;

            framesAccumulated = 0;
            timeAccumulated = 0f;
        }
    }
}