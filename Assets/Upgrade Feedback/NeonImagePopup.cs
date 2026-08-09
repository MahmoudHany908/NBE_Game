using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to your Canvas (or any GameObject). Wire a Button's OnClick to
/// Popup(). The target Image keeps ONE material at all times (using the
/// NeonUnlitHDR shader) — clicking the button fades it from its normal
/// look into bright HDR neon and back over ~1 second. Because it's a
/// blend on a single material rather than a material swap, it actually
/// crossfades instead of popping instantly.
///
/// Setup (matches: Canvas > Image, Canvas > Button):
///  1. Create a Material using Shader "Custom/URP/NeonUnlitHDR".
///  2. Set that material's "Base Color" to match your Image's current
///     look (e.g. white, or whatever tint it normally has).
///  3. Assign that material as the Image's Material (Image component's
///     "Material" slot, or drag onto the Image GameObject).
///  4. Assign targetImage = your Image, triggerButton = your Button.
///  5. Enable Bloom on your URP Volume so the neon spike actually glows.
/// </summary>
[DisallowMultipleComponent]
public class NeonImagePopup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Button that triggers the neon fade.")]
    [SerializeField] private Button triggerButton;

    [Tooltip("The Image using the NeonUnlitHDR material (permanently).")]
    [SerializeField] private Image targetImage;

    [Header("Timing")]
    [Tooltip("Total duration of the fade in + fade out, in seconds.")]
    [SerializeField] private float popDuration = 1f;

    [Tooltip("Fraction of popDuration spent fading INTO neon before fading back out.")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float fadeInPortion = 0.35f;

    [Header("Neon Look")]
    [SerializeField] private float restIntensity = 1f;
    [SerializeField] private float peakIntensity = 6f;
    [ColorUsage(true, true)]
    [SerializeField] private Color neonColor = new Color(0f, 3f, 3f, 1f); // HDR cyan

    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int NeonColorID = Shader.PropertyToID("_NeonColor");
    private static readonly int BlendID = Shader.PropertyToID("_Blend");

    private Material materialInstance;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (triggerButton != null)
            triggerButton.onClick.AddListener(Popup);

        if (targetImage != null)
        {
            // Instance the material so we don't edit the shared asset at runtime.
            materialInstance = Instantiate(targetImage.material);
            targetImage.material = materialInstance;

            materialInstance.SetColor(NeonColorID, neonColor);
            materialInstance.SetFloat(BlendID, 0f);          // start fully "default"
            materialInstance.SetFloat(IntensityID, restIntensity);
        }
    }

    /// <summary>Call this from a Button's OnClick event.</summary>
    public void Popup()
    {
        if (materialInstance == null)
        {
            Debug.LogWarning("[NeonImagePopup] Missing targetImage reference or material.");
            return;
        }

        if (!gameObject.activeInHierarchy) return;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlayFade());
    }

    private IEnumerator PlayFade()
    {
        float fadeInTime = popDuration * fadeInPortion;
        float fadeOutTime = popDuration - fadeInTime;

        // Fade IN: default -> neon
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeInTime);
            float eased = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic
            materialInstance.SetFloat(BlendID, eased);
            materialInstance.SetFloat(IntensityID, Mathf.Lerp(restIntensity, peakIntensity, eased));
            yield return null;
        }

        // Fade OUT: neon -> default
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeOutTime);
            float eased = p * p; // ease-in quad
            materialInstance.SetFloat(BlendID, 1f - eased);
            materialInstance.SetFloat(IntensityID, Mathf.Lerp(peakIntensity, restIntensity, eased));
            yield return null;
        }

        materialInstance.SetFloat(BlendID, 0f);
        materialInstance.SetFloat(IntensityID, restIntensity);
        activeRoutine = null;
    }
}
