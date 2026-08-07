using LitMotion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single popup notification entry in the loot feed.
/// Slides in, lingers, then fades out and self-destructs.
/// Place this on a prefab with a TextMeshProUGUI child and a CanvasGroup.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class LootPopupEntry : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float slideDistance = 80f;
    [SerializeField] private float slideInDuration = 0.25f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;

    private CanvasGroup _canvasGroup;
    private RectTransform _rect;
    private readonly List<MotionHandle> _handles = new List<MotionHandle>(3);

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rect = (RectTransform)transform;

        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();

        //if (label == null)
            //Debug.LogError($"[{nameof(LootPopupEntry)}] Missing TextMeshProUGUI reference on {gameObject.name}.", this);
    }

    /// <summary>
    /// Initializes the popup with the given text and an optional icon.
    /// </summary>
    public void Initialize(string text, Sprite iconSprite = null)
    {
        if (label != null)
            label.text = text;

        if (icon != null)
        {
            if (iconSprite != null)
            {
                icon.sprite = iconSprite;
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }

        _canvasGroup.alpha = 0f;
        _rect.localScale = Vector3.one * 0.7f;

        // Scale pop-in (does not fight VerticalLayoutGroup)
        _handles.Add(
            LMotion.Create(0.7f, 1f, slideInDuration)
                .WithEase(Ease.OutBack)
                .Bind(v => _rect.localScale = new Vector3(v, v, 1f))
        );

        // Fade in
        _handles.Add(
            LMotion.Create(0f, 1f, slideInDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(StartFadeOut)
                .Bind(v => _canvasGroup.alpha = v)
        );
    }

    private void StartFadeOut()
    {
        // Fade out after display duration, then destroy
        _handles.Add(
            LMotion.Create(1f, 0f, fadeOutDuration)
                .WithDelay(displayDuration)
                .WithEase(Ease.InQuad)
                .WithOnComplete(DestroySelf)
                .Bind(v => _canvasGroup.alpha = v)
        );
    }

    private void DestroySelf()
    {
        CancelAll();
        Destroy(gameObject);
    }

    private void CancelAll()
    {
        for (int i = 0; i < _handles.Count; i++)
        {
            if (_handles[i].IsActive())
                _handles[i].Cancel();
        }
        _handles.Clear();
    }

    private void OnDisable()
    {
        CancelAll();
    }
}
