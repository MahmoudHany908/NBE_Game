using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour, IPoolable
{
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.85f, 0f);
    [SerializeField] private Color healColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color missColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Float")]
    [SerializeField] private float floatDistance = 1.5f;
    [SerializeField] private Ease floatEase = Ease.OutQuad;

    [Header("Timing")]
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float punchInDuration = 0.12f;
    [SerializeField] private float punchSettleDuration = 0.09f;
    [SerializeField] private float scalePunch = 1.3f;


    // ─── Cached refs ───
    private TextMeshPro _tmp;
    private Vector3 _baseScale;

    private readonly List<MotionHandle> _handles = new List<MotionHandle>(4);
    private PooledObject pooledObject;
    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();

        _tmp = GetComponent<TextMeshPro>();
        _baseScale = transform.localScale;
    }

    // ──────────────── IPoolable ────────────────

    public void OnSpawned()
    {
        _tmp.alpha = 1f;
        transform.localScale = Vector3.zero;
    }

    public void OnDespawned()
    {
        CancelAll();
    }

    // ──────────────── Public API ────────────────

    public void Play(float value) => PlayInternal(Format(value), normalColor);
    public void PlayCritical(float value) => PlayInternal(Format(value), criticalColor);
    public void PlayHeal(float value) => PlayInternal("+" + Format(value), healColor);
    public void PlayMiss() => PlayInternal("MISS", missColor);
    public void Play(float value, Color color) => PlayInternal(Format(value), color);
    public void Play(string text, Color color) => PlayInternal(text, color);

    // ──────────────── Core ────────────────

    private void PlayInternal(string text, Color color)
    {
        CancelAll();

        _tmp.text = text;
        _tmp.color = color;
        _tmp.alpha = 1f;
        transform.localScale = Vector3.zero;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;
        Color transparent = new Color(color.r, color.g, color.b, 0f);
        float fadeDelay = Mathf.Max(0f, lifetime - fadeDuration);

        // Float up
        _handles.Add(
            LMotion.Create(startPos, endPos, lifetime)
                .WithEase(floatEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(v => transform.position = v)
        );

        // Fade out — triggers pool return on complete
        _handles.Add(
            LMotion.Create(color, transparent, fadeDuration)
                .WithDelay(fadeDelay)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(ReturnToPool)
                .Bind(v => _tmp.color = v)
        );

        // Scale punch-in
        _handles.Add(
            LMotion.Create(0f, scalePunch, punchInDuration)
                .WithEase(Ease.OutBack)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(PlayPunchSettle)
                .Bind(v => transform.localScale = _baseScale * v)
        );
    }

    private void PlayPunchSettle()
    {
        _handles.Add(
            LMotion.Create(scalePunch, 1f, punchSettleDuration)
                .WithEase(Ease.InQuad)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(v => transform.localScale = _baseScale * v)
        );
    }

    private void ReturnToPool()
    {
        CancelAll();
        pooledObject.ReturnToPool();
    }

    // ──────────────── Internals ────────────────



    private static string Format(float value) => Mathf.RoundToInt(value).ToString();

    private void CancelAll()
    {
        for (int i = 0; i < _handles.Count; i++)
            if (_handles[i].IsActive()) _handles[i].Cancel();
        _handles.Clear();
    }

    private void OnDisable() => CancelAll();
}