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
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button highButton;

    [Header("Selection Highlights (optional)")]
    [Tooltip("Image to show when Low is active")]
    [SerializeField] private GameObject lowHighlight;
    [Tooltip("Image to show when Medium is active")]
    [SerializeField] private GameObject mediumHighlight;
    [Tooltip("Image to show when High is active")]
    [SerializeField] private GameObject highHighlight;

    private GraphicsSettingsManager _graphicsManager;

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
        if (mediumButton != null) mediumButton.onClick.AddListener(OnMediumClicked);
        if (highButton   != null) highButton.onClick.AddListener(OnHighClicked);

        RefreshHighlights();
    }

    private void OnDisable()
    {
        if (lowButton    != null) lowButton.onClick.RemoveListener(OnLowClicked);
        if (mediumButton != null) mediumButton.onClick.RemoveListener(OnMediumClicked);
        if (highButton   != null) highButton.onClick.RemoveListener(OnHighClicked);
    }

    // ── Button Callbacks ─────────────────────────────────────────────────────
    private void OnLowClicked()
    {
        _graphicsManager?.ApplyQuality(GraphicsQuality.Low);
        RefreshHighlights();
    }

    private void OnMediumClicked()
    {
        _graphicsManager?.ApplyQuality(GraphicsQuality.Medium);
        RefreshHighlights();
    }

    private void OnHighClicked()
    {
        _graphicsManager?.ApplyQuality(GraphicsQuality.High);
        RefreshHighlights();
    }

    // ── Visual State ─────────────────────────────────────────────────────────
    private void RefreshHighlights()
    {
        if (_graphicsManager == null) return;

        var current = _graphicsManager.CurrentQuality;

        SetHighlight(lowHighlight,    current == GraphicsQuality.Low);
        SetHighlight(mediumHighlight, current == GraphicsQuality.Medium);
        SetHighlight(highHighlight,   current == GraphicsQuality.High);
    }

    private static void SetHighlight(GameObject highlight, bool active)
    {
        if (highlight != null) highlight.SetActive(active);
    }
}
