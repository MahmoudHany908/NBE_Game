using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

/// <summary>
/// Centralized Graphics Quality Manager.
/// 
/// Responsibilities:
///   - Apply Low / Medium / High quality presets at startup and on demand.
///   - Toggle the Global Volume (expensive post-processing).
///   - Persist the chosen preset via ISaveSystem (GameData.graphics).
/// 
/// Preset definitions:
///   LOW    – HDR off, Global Volume off
///   MEDIUM – HDR off, Global Volume on
///   HIGH   – HDR on,  Global Volume on
/// 
/// Attach to: a dedicated "GraphicsManager" GameObject that persists across scenes
///            (or register it as a VContainer singleton in GameLifetimeScope).
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Volume")]
    [Tooltip("Assign the Global Volume GameObject from the scene.")]
    [SerializeField] private Volume globalVolume;

    [Header("Default Quality")]
    [SerializeField] private GraphicsQuality defaultQuality = GraphicsQuality.Medium;

    // ── Private state ────────────────────────────────────────────────────────
    private ISaveSystem _saveSystem;
    private GraphicsQuality _currentQuality;

    // ── VContainer Injection ─────────────────────────────────────────────────
    [Inject]
    public void Construct(ISaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    private void Start()
    {
        // Auto-locate the Volume if not assigned in Inspector
        if (globalVolume == null)
            globalVolume = FindAnyObjectByType<Volume>(FindObjectsInactive.Include);

        LoadAndApplySavedQuality();
    }

    // ── Public API ───────────────────────────────────────────────────────────
    /// <summary>Apply a quality preset and persist the choice.</summary>
    public void ApplyQuality(GraphicsQuality quality)
    {
        if (_currentQuality == quality) return;

        _currentQuality = quality;
        ApplyPreset(quality);
        SaveQuality(quality);
    }

    public GraphicsQuality CurrentQuality => _currentQuality;

    // ── Internal Logic ───────────────────────────────────────────────────────
    private void LoadAndApplySavedQuality()
    {
        GraphicsQuality savedQuality = defaultQuality;

        if (_saveSystem != null)
        {
            var data = _saveSystem.Load();
            if (data?.graphics != null)
                savedQuality = (GraphicsQuality)data.graphics.qualityLevel;
        }

        _currentQuality = savedQuality;
        ApplyPreset(savedQuality);
    }

    private void ApplyPreset(GraphicsQuality quality)
    {
        switch (quality)
        {
            case GraphicsQuality.Low:
                SetHDR(false);
                SetVolume(false);
                break;

            case GraphicsQuality.Medium:
                SetHDR(false);
                SetVolume(true);
                break;

            case GraphicsQuality.High:
                SetHDR(true);
                SetVolume(true);
                break;
        }

        Debug.Log($"[GraphicsSettingsManager] Applied preset: {quality}");
    }

    // ── HDR ──────────────────────────────────────────────────────────────────
    private void SetHDR(bool enabled)
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogWarning("[GraphicsSettingsManager] No URP asset found.");
            return;
        }

        // UniversalRenderPipelineAsset.supportsHDR is writable at runtime.
        urpAsset.supportsHDR = enabled;
    }

    // ── Global Volume ────────────────────────────────────────────────────────
    private void SetVolume(bool enabled)
    {
        // 1. Toggle the volume component itself (and its weight to force instant visual update)
        if (globalVolume != null)
        {
            globalVolume.enabled = enabled;
            globalVolume.weight = enabled ? 1f : 0f;
        }
        else
        {
            Debug.LogWarning("[GraphicsSettingsManager] GlobalVolume reference is null — assign it in Inspector or ensure a Volume exists in scene.");
        }

        // 2. Directly toggle the Post-Processing pass on the Main Camera.
        // This is the most robust way to ensure it updates instantly AND it completely 
        // removes the post-process pass from URP, saving maximum GPU time on mobile.
        if (Camera.main != null)
        {
            var camData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null)
            {
                camData.renderPostProcessing = enabled;
            }
        }
    }

    // ── Persistence ──────────────────────────────────────────────────────────
    private void SaveQuality(GraphicsQuality quality)
    {
        if (_saveSystem == null) return;
        var data = _saveSystem.Load();
        if (data?.graphics == null) return;
        data.graphics.qualityLevel = (int)quality;
        _saveSystem.Save(data);
    }
}
