using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// UI controller for the Graphics Quality selection inside the Settings menu.
/// 
/// Wires three buttons (Low / Medium / High) to GraphicsSettingsManager.
/// Highlights the active preset button visually.
/// 
/// Attach to: the Settings panel root GameObject (alongside SettingsMenu.cs),
///            OR a child panel dedicated to graphics settings.
/// 
/// Inspector references:
///   - lowButton, mediumButton, highButton → UnityEngine.UI.Button
///   - lowHighlight, mediumHighlight, highHighlight → any Image acting as a
///     selected-state indicator (outline, glow, tinted background, etc.)
/// </summary>
public class GraphicsQualityUI : MonoBehaviour
{
    [Header("Quality Buttons")]
    [SerializeField] private Button lowButton;
    [SerializeField] private Button highButton;

    [Header("Selection Highlights (optional)")]
    [Tooltip("Image to show when Low is active")]
    [SerializeField] private GameObject lowHighlight;
    [Tooltip("Image to show when High is active")]
    [SerializeField] private GameObject highHighlight;

    [Header("HDR Settings")]
    [Tooltip("Toggle for HDR and Bloom")]
    [SerializeField] private Toggle hdrToggle;
    [Tooltip("Popup prompting the user to restart to apply HDR changes")]
    [SerializeField] private GameObject restartPopup;

    private GraphicsSettingsManager _graphicsManager;
    private Coroutine _popupCoroutine;

    // ── VContainer Injection ─────────────────────────────────────────────────
    [Inject]
    public void Construct(GraphicsSettingsManager graphicsManager)
    {
        _graphicsManager = graphicsManager;
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        if (lowButton    != null) lowButton.onClick.AddListener(OnLowClicked);
        if (highButton   != null) highButton.onClick.AddListener(OnHighClicked);

        if (hdrToggle != null)
        {
            // Set the toggle to match the current saved data state
            hdrToggle.SetIsOnWithoutNotify(_graphicsManager != null && _graphicsManager.IsHDREnabled);
            hdrToggle.onValueChanged.AddListener(OnHDRToggleChanged);
        }

        if (restartPopup != null)
        {
            restartPopup.SetActive(false);
        }

        RefreshHighlights();
    }

    private void OnDisable()
    {
        if (lowButton    != null) lowButton.onClick.RemoveListener(OnLowClicked);
        if (highButton   != null) highButton.onClick.RemoveListener(OnHighClicked);

        if (hdrToggle != null) hdrToggle.onValueChanged.RemoveListener(OnHDRToggleChanged);
    }

    // ── Button Callbacks ─────────────────────────────────────────────────────
    private void OnLowClicked()
    {
        _graphicsManager?.ApplyQuality(GraphicsQuality.Low);
        RefreshHighlights();
    }

    private void OnHighClicked()
    {
        _graphicsManager?.ApplyQuality(GraphicsQuality.High);
        RefreshHighlights();
    }

    private void OnHDRToggleChanged(bool isOn)
    {
        _graphicsManager?.SetHDREnabled(isOn);
        
        // Only prompt restart when ENABLING HDR — disabling applies immediately.
        if (isOn && restartPopup != null)
        {
            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(ShowAndFadePopup());
        }
    }

    private System.Collections.IEnumerator ShowAndFadePopup()
    {
        restartPopup.SetActive(true);
        
        // Ensure we have a CanvasGroup to handle the fading
        var canvasGroup = restartPopup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = restartPopup.AddComponent<CanvasGroup>();
            
        canvasGroup.alpha = 1f;
        
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);
        
        // Fade out over 0.5 seconds
        float elapsed = 0f;
        float fadeTime = 0.5f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        restartPopup.SetActive(false);
        canvasGroup.alpha = 1f; // Reset alpha for next time it appears
    }

    // ── Visual State ─────────────────────────────────────────────────────────
    private void RefreshHighlights()
    {
        if (_graphicsManager == null) return;

        var current = _graphicsManager.CurrentQuality;

        SetHighlight(lowHighlight,    current == GraphicsQuality.Low);
        SetHighlight(highHighlight,   current == GraphicsQuality.High);
    }

    private static void SetHighlight(GameObject highlight, bool active)
    {
        if (highlight != null) highlight.SetActive(active);
    }
}
